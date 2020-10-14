using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public override async Task<string> GetRefreshToken()
        {
            if (File.Exists(_cacheFilePath))
            {
                AppData.Logger.LogDebug("Found cache file.");
                var cachedRefreshToken =
                    Encoding.GetString(_dataProtector.Unprotect(await File.ReadAllBytesAsync(_cacheFilePath)));
                if (!cachedRefreshToken.Equals(string.Empty))
                    return cachedRefreshToken;

                AppData.Logger.LogInformation("Token cache invalid. You have to reauthenticate.");
                File.Delete(_cacheFilePath);
            }

            var token = await base.GetRefreshToken();
            Directory.CreateDirectory(Path.GetDirectoryName(_cacheFilePath));
            await File.WriteAllBytesAsync(_cacheFilePath,
                _dataProtector.Protect(Encoding.GetBytes(token))); // cache token
            AppData.Logger.LogDebug("Cached token.");
            return token;
        }
    }
}