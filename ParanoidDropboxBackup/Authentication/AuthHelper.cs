using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using ParanoidDropboxBackup.App;

namespace ParanoidDropboxBackup.Authentication
{
    public class AuthHelper
    {
        private const string LoopbackHost = "http://localhost:52443/";
        private static readonly Uri RedirectUri = new Uri(LoopbackHost + "authorize");
        private static readonly Uri JsRedirectUri = new Uri(LoopbackHost + "token");

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

            var http = new HttpListener();
            http.Prefixes.Add(LoopbackHost);
            http.Start();

            Console.WriteLine(authorizeUri.ToString()); // print auth url

            HandleOAuth2Redirect(http).GetAwaiter().GetResult(); // send html to redirect url

            var result = HandleJsRedirect(http).GetAwaiter().GetResult(); // get token from href

            if (result.State != state) throw new OAuth2Exception("Unexpected state.");

            if (result.AccessToken.Equals(string.Empty)) throw new OAuth2Exception("Token empty.");

            AppData.Logger.LogDebug("Authentication through OAuth2 successful.");

            return result.AccessToken;
        }


        /// <summary>
        ///     Handles the redirect from Dropbox server. Because we are using token flow, the local
        ///     http server cannot directly receive the URL fragment. We need to return a HTML page with
        ///     inline JS which can send URL fragment to local server as URL parameter.
        /// </summary>
        /// <param name="http">The http listener.</param>
        /// <returns>The <see cref="Task" /></returns>
        private static async Task HandleOAuth2Redirect(HttpListener http)
        {
            var context = await http.GetContextAsync();

            // We only care about request to RedirectUri endpoint.
            while (context.Request.Url.AbsolutePath != RedirectUri.AbsolutePath) context = await http.GetContextAsync();

            context.Response.ContentType = "text/html";

            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            await using (var reader = embeddedProvider
                .GetFileInfo(Path.Combine("Resources", Constants.RedirectHtmlFileName))
                .CreateReadStream())
            {
                await reader.CopyToAsync(context.Response.OutputStream);
            }

            context.Response.OutputStream.Close();
        }

        /// <summary>
        ///     Handle the redirect from JS and process raw redirect URI with fragment to
        ///     complete the authorization flow.
        /// </summary>
        /// <param name="http">The http listener.</param>
        /// <returns>The <see cref="OAuth2Response" /></returns>
        private static async Task<OAuth2Response> HandleJsRedirect(HttpListener http)
        {
            var context = await http.GetContextAsync();

            // We only care about request to TokenRedirectUri endpoint.
            while (context.Request.Url.AbsolutePath != JsRedirectUri.AbsolutePath)
                context = await http.GetContextAsync();

            var redirectUri = new Uri(context.Request.QueryString["url_with_fragment"]);

            var result = DropboxOAuth2Helper.ParseTokenFragment(redirectUri);

            return result;
        }
    }
}