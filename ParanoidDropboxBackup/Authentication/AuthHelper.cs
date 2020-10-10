using System;
using System.Net;
using Dropbox.Api;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Authentication
{
    public class AuthHelper
    {
        private const string RedirectUri = "http://localhost:54878/authorize";

        private readonly string _appKey;

        public AuthHelper(string appKey)
        {
            _appKey = appKey;
        }

        public virtual string GetAuthToken()
        {
            AppData.Logger.LogDebug("Getting url for authentication through OAuth2.");

            var state = Guid.NewGuid().ToString("N");
            var authorizeUri =
                DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, _appKey, RedirectUri, state);

            Console.WriteLine(
                "Open {0} in your browser and grant access to your dropbox. Then paste the url to which you are directed to in this console window.",
                authorizeUri); // print auth url

            var result =
                DropboxOAuth2Helper.ParseTokenFragment(new Uri(Console.ReadLine() ??
                                                               throw new InvalidOperationException()));

            if (result.State != state) throw new OAuth2Exception("Unexpected state.");

            if (result.AccessToken.Equals(string.Empty)) throw new OAuth2Exception("Token empty.");

            AppData.Logger.LogDebug("Authentication through OAuth2 successful.");

            return result.AccessToken;
        }
    }
}