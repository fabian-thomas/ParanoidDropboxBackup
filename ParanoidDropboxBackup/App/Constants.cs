using System;
using System.IO;

namespace ParanoidDropboxBackup.App
{
    internal static class Constants
    {
        public const string AppTitle = "ParanoidDropboxBackup";
        public const string ConfigFileName = "config.json";
        public const string IgnoreFileName = "ignore";
        public const string RedirectHtmlFileName = "index.html";
        private const string TokenCacheFileName = "token_cache";
        public const string BackupDirPrefix = "Dropbox_";

        /*
         * Linux: home/<user>/.config/ParanoidDropboxBackup
         * Windows: %localappdata%/ParanoidDropboxBackup
         */
        public static string AppDataFolderPath =>
            Path.Combine(
                Environment.OSVersion.Platform == PlatformID.Win32NT
                    ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppTitle);

        public static string ConfigFilePath => Path.Combine(AppDataFolderPath, ConfigFileName);

        public static string IgnoreFilePath => Path.Combine(AppDataFolderPath, IgnoreFileName);

        /*
         * Linux: home/<user>/.cache/ParanoidDropboxBackup
         * Windows: %localappdata%/ParanoidDropboxBackup
         */
        private static string CacheFolderPath
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".cache",
                        AppTitle);
                else
                    return AppDataFolderPath;
            }
        }

        public static string TokenCacheFilePath => Path.Combine(CacheFolderPath, TokenCacheFileName);
    }
}