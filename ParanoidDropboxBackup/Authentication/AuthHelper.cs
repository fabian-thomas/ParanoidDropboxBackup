using System;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Authentication
{
    public class AuthHelper
    {
        private readonly string _appKey;

        protected AuthHelper(string appKey)
        {
            _appKey = appKey;
        }

        public virtual async Task<string> GetRefreshToken()
        {
            AppData.Logger.LogDebug("Getting url for authentication through OAuth2.");

            var authFlow = new PKCEOAuthFlow();
            var authorizeUri = authFlow.GetAuthorizeUri(OAuthResponseType.Code, _appKey, string.Empty,
                tokenAccessType: TokenAccessType.Offline);

            Console.WriteLine(
                "Open {0} in your browser and grant access to your dropbox. Paste the resulting code in this terminal.",
                authorizeUri); // print auth url

            var code = Console.ReadLine() ??
                       throw new InvalidOperationException();

            var result = await authFlow.ProcessCodeFlowAsync(code, _appKey);
            AppData.Logger.LogDebug("Exchanged code for token and refresh token.");

            if (result.AccessToken == null || result.AccessToken.Equals(string.Empty))
                throw new OAuth2Exception("Token empty.");
            if (result.RefreshToken == null || result.RefreshToken.Equals(string.Empty))
                throw new OAuth2Exception("Refresh token empty.");

            AppData.Logger.LogDebug("Authentication through OAuth2 successful.");

            return result.RefreshToken;
        }
    }
}