using System.ComponentModel;

namespace ParanoidDropboxBackup.App.Configuration
{
    [Description("Backup")]
    internal class BackupConfig
    {
        public string Path { get; set; }
        public int RemainMaximum { get; set; }
        public uint MaxParallelDownloadTasks { get; set; }
        public ProgressReporting ProgressReporting { get; set; }
    }
}
