using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Dropbox
{
    public class DropboxHelper
    {
        private readonly CancellationToken _ct;
        private readonly DropboxClient _dropboxClient;
        private readonly uint _maxParallelDownloadTasks;
        private readonly bool _reportingEnabled;
        private readonly decimal _reportingSteps;
        private readonly string _rootPath;

        public DropboxHelper(string token, CancellationToken ct,
            string rootPath, uint maxParallelDownloadTasks, decimal reportingSteps, bool reportingEnabled)
        {
            _dropboxClient = new DropboxClient(token);
            _ct = ct;
            _rootPath = rootPath;
            _maxParallelDownloadTasks = maxParallelDownloadTasks;
            _reportingSteps = reportingSteps;
            _reportingEnabled = reportingEnabled;
        }

        public async Task DownloadAll()
        {
            // try
            // {
            AppData.Logger.LogInformation("Backing up Dropbox to \"{0}\"", _rootPath);

            var downloadVisitor = _reportingEnabled
                ? new ReportingDownloadVisitor(AppData.Ignore, _rootPath, _reportingSteps, _dropboxClient, _ct)
                : new DownloadVisitor(AppData.Ignore, _dropboxClient, _ct, _rootPath);
            await downloadVisitor.Init();

            var iteration =
                new IterationContentVisitor(downloadVisitor, _dropboxClient, _ct, _maxParallelDownloadTasks);
            await iteration.Iterate();

            AppData.Logger.LogInformation("Backup finished.");
            // }
            // catch (ServiceException ex)
            // {
            //     AppData.Logger.LogCritical("Could not get root item of OneDrive. Nothing could be downloaded.\n{0}",
            //         ex);
            // }
        }
    }
}