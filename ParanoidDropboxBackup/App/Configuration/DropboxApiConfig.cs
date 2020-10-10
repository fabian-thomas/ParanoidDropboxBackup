using System.ComponentModel;

namespace ParanoidDropboxBackup.App.Configuration
{
    [Description("DropboxAPI")]
    internal class DropboxApiConfig
    {
        public string AppKey { get; set; }
    }
}
