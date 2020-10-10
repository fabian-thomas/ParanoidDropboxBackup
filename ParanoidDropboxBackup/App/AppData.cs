using MAB.DotIgnore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App.Configuration;

namespace ParanoidDropboxBackup.App
{
    internal static class AppData
    {
        public static readonly DropboxApiConfig DropboxApiConfig = new DropboxApiConfig();
        public static readonly BackupConfig BackupConfig = new BackupConfig();
        public static IHostApplicationLifetime Lifetime;
        public static IDataProtector Protector;
        public static IgnoreList Ignore;
        public static ILogger<BackupService> Logger;

        public static void BindConfig(IConfiguration config)
        {
            config.Bind(Helper.GetDescription(typeof(DropboxApiConfig)), DropboxApiConfig);
            config.Bind(Helper.GetDescription(typeof(BackupConfig)), BackupConfig);


            // TODO check if values in config are correct
        }
    }
}
