using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Dropbox
{
    public class IterationContentVisitor : IContentVisitor
    {
        private readonly CancellationToken _ct;
        private readonly DropboxClient _dropboxClient;
        private readonly uint _maxParallelFiles;
        private readonly List<Task> _tasks = new List<Task>();
        private readonly IContentVisitor _visitor;

        public IterationContentVisitor(IContentVisitor visitor, DropboxClient dropboxClient, CancellationToken ct,
            uint maxParallelFiles = 0)
        {
            _visitor = visitor;
            _dropboxClient = dropboxClient;
            _ct = ct;
            _maxParallelFiles = maxParallelFiles;
        }

        public Task Visit(FileMetadata file)
        {
            if (_maxParallelFiles != 0)
            {
                _tasks.RemoveAll(x => x.IsCompleted);
                // wait for tasks to finish
                if (_tasks.Count >= _maxParallelFiles)
                {
                    AppData.Logger.LogDebug(
                        "Maximum number of tasks reached. Awaiting any task to finish.");
                    var index = Task.WaitAny(_tasks.ToArray(), _ct);
                    if (index >= 0 && index < _tasks.Count)
                        _tasks.RemoveAt(index);
                }
            }

            _tasks.Add(_visitor.Visit(file));
            return Task.CompletedTask;
        }

        public async Task Visit(FolderMetadata folder)
        {
            _tasks.RemoveAll(x => x.IsCompleted);

            await _visitor.Visit(folder);

            ListFolderResult children = null;
            do
            {
                if (children == null)
                    children = await _dropboxClient.Files.ListFolderAsync(folder.PathLower,
                        includeNonDownloadableFiles: false);
                else
                    children = await _dropboxClient.Files
                        .ListFolderContinueAsync(children.Cursor); // use cursor to get the next files

                foreach (var entry in children.Entries)
                    await Accept(entry);
            } while (children.HasMore);
        }

        public async Task Visit(DeletedMetadata deleted)
        {
            await _visitor.Visit(deleted);
        }

        public async Task Iterate()
        {
            ListFolderResult root = null;
            do
            {
                if (root == null)
                    root = await _dropboxClient.Files.ListFolderAsync(string.Empty, includeNonDownloadableFiles: false);
                else
                    root = await _dropboxClient.Files
                        .ListFolderContinueAsync(root.Cursor); // use cursor to get the next files

                foreach (var entry in root.Entries)
                    await Accept(entry);
            } while (root.HasMore);

            Task.WaitAll(_tasks.ToArray(), _ct);
        }

        private async Task Accept(Metadata metadata)
        {
            if (metadata.IsFile)
                await Visit(metadata.AsFile);
            else if (metadata.IsDeleted)
                await Visit(metadata.AsDeleted);
            else
                await Visit(metadata.AsFolder);
        }
    }
}