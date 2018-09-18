using System;
using System.Collections.Generic;
using System.Configuration;

using System.Linq;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

using Owin;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Web;
using PocAadClient.Utils;

namespace PocAadClient
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string appKey = ConfigurationManager.AppSettings["ida:Password"];

        public static readonly string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];


        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
      
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());


            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = postLogoutRedirectUri,
                
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = OnAuthenticationFailed,
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived

                    }
                });

        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            var code = context.Code;

            ClientCredential credential = new ClientCredential(clientId, appKey);
            string userObjectID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            


            AuthenticationContext authContext = new AuthenticationContext(authority,false);
            //AuthenticationContext authContext = new AuthenticationContext(authority, new NaiveSessionCache(userObjectID));

            // If you create the redirectUri this way, it will contain a trailing slash.  
            // Make sure you've registered the same exact Uri in the Azure Portal (including the slash).
            Uri uri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));

            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, uri, credential, graphResourceId);
        }

    }
}
