using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using MAB.DotIgnore;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Dropbox
{
    public class DownloadVisitor : IContentVisitor
    {
        private readonly CancellationToken _ct;
        private readonly DropboxClient _dropboxClient;
        private readonly IgnoreList _ignore;
        private readonly string _rootPath;

        public DownloadVisitor(IgnoreList ignore, DropboxClient dropboxClient, CancellationToken ct, string rootPath)
        {
            _ignore = ignore;
            _rootPath = rootPath;
            _ct = ct;
            _dropboxClient = dropboxClient;
        }

        public virtual async Task Visit(FileMetadata file)
        {
            // check ignore
            if (AppData.Ignore.IsIgnored(file.PathDisplay, false))
            {
                AppData.Logger.LogDebug("Ignoring file: \"{0}\"", file.PathDisplay);
            }
            else
            {
                // download file
                var fileStream =
                    File.Create(Path.Combine(_rootPath, file.PathDisplay.Remove(0, 1))); // remove leading slash

                var contentStream = await (await _dropboxClient.Files.DownloadAsync(file.PathLower))
                    .GetContentAsStreamAsync();

                if (_ct.IsCancellationRequested) return;

                AppData.Logger.LogDebug("Downloading file: \"{0}\"", file.PathDisplay);

                await contentStream.CopyToAsync(fileStream, _ct);
                fileStream.Close();
            }
        }

        public Task Visit(FolderMetadata folder)
        {
            // check ignore
            if (_ignore.IsIgnored(folder.PathDisplay, true))
            {
                AppData.Logger.LogDebug("Ignoring folder: \"{0}\"", folder.PathDisplay);
                return Task.CompletedTask;
            }

            try
            {
                Directory.CreateDirectory(Path.Combine(_rootPath,
                    folder.PathDisplay.Remove(0, 1))); // remove leading slash
                AppData.Logger.LogDebug("Created Directory: \"{0}\"", folder.PathLower);
            }
            catch (IOException ex)
            {
                AppData.Logger.LogError(
                    "Could not create directory \"{0}\". No children have been downloaded.\n{1}",
                    folder.PathDisplay, ex);
            }

            return Task.CompletedTask;
        }

        public Task Visit(DeletedMetadata deleted)
        {
            return Task.CompletedTask;
        }

        public virtual Task Init()
        {
            Directory.CreateDirectory(_rootPath);
            return Task.CompletedTask;
        }
    }
}