using System.IO;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Authentication
{
    public class CachedAuthHelper : AuthHelper
    {
        private static readonly Encoding Encoding = Encoding.ASCII;

        private readonly string _cacheFilePath;
        private readonly IDataProtector _dataProtector;

        public CachedAuthHelper(string appKey, string cacheFilePath, IDataProtector dataProtector) : base(appKey)
        {
            _cacheFilePath = cacheFilePath;
            _dataProtector = dataProtector;
        }

        public override string GetAuthToken()
        {
            if (File.Exists(_cacheFilePath))
            {
                AppData.Logger.LogDebug("Found cache file.");
                var cachedToken = Encoding.GetString(_dataProtector.Unprotect(File.ReadAllBytes(_cacheFilePath)));
                if (!cachedToken.Equals(string.Empty))
                    return cachedToken;

                AppData.Logger.LogInformation("Token cache invalid. You have to reauthenticate.");
                File.Delete(_cacheFilePath);
            }

            var token = base.GetAuthToken();
            Directory.CreateDirectory(Path.GetDirectoryName(_cacheFilePath));
            File.WriteAllBytes(_cacheFilePath, _dataProtector.Protect(Encoding.GetBytes(token))); // cache token
            AppData.Logger.LogDebug("Cached token.");
            return token;
        }
    }
}