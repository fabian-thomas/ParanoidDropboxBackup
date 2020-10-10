using System;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using MAB.DotIgnore;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Dropbox
{
    public class ReportingDownloadVisitor : DownloadVisitor
    {
        private readonly CancellationToken _ct;
        private readonly DropboxClient _dropboxClient;
        private readonly decimal _reportingSteps;
        private ulong _downloaded;
        private decimal _lastReported;
        private ulong _total;

        public ReportingDownloadVisitor(IgnoreList ignore, string rootPath, decimal reportingSteps,
            DropboxClient dropboxClient, CancellationToken ct) :
            base(ignore, dropboxClient, ct, rootPath)
        {
            _reportingSteps = reportingSteps;
            _dropboxClient = dropboxClient;
            _ct = ct;
        }

        private async Task<ulong> CalcTotalSize()
        {
            ulong total = 0;
            ListFolderResult root = null;
            do
            {
                if (root == null)
                    root = await _dropboxClient.Files.ListFolderAsync(string.Empty, true,
                        includeNonDownloadableFiles: false);
                else
                    root = await _dropboxClient.Files.ListFolderContinueAsync(root.Cursor);

                foreach (var entry in root.Entries)
                    if (entry.IsFile)
                        total += entry.AsFile.Size;
            } while (root.HasMore);

            return total;
        }

        public override async Task Init()
        {
            _total = await CalcTotalSize();

            await base.Init();
        }

        public override async Task Visit(FileMetadata file)
        {
            await base.Visit(file);

            ReportProgress(file.Size);
        }

        private void ReportProgress(ulong finishedDownloading)
        {
            if (_total == 0) return;

            _downloaded += finishedDownloading;
            var div = decimal.Divide(_downloaded, _total);

            if (div.CompareTo(_lastReported + _reportingSteps) == -1) return;

            _lastReported = _reportingSteps * Math.Floor(div / _reportingSteps);
            AppData.Logger.LogInformation(
                $"{_lastReported * 100}% - {_downloaded / 1000000}/{_total / 1000000}Mb");
        }
    }
}