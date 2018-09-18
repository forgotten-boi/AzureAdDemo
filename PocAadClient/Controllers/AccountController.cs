

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// The following using statements were added for this sample.
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Configuration;
using System.Globalization;
using PocAadClient.Utils;

namespace PocAadClient.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn()
        {
            // Send an OpenID Connect sign-in request.
            if (!Request.IsAuthenticated)
            {
                
                System.Web.HttpContext.Current.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }
        [Authorize]
        public void SignOut()
        {
            // Remove all cache entries for this user and send an OpenID Connect sign-out request.
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.authority, new NaiveSessionCache(userObjectID));
            authContext.TokenCache.Clear();

            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }
        
        public void EndSession()
        {
            if (HttpContext.Request.IsAuthenticated)
            {
                // Remove all cache entries for this user and send an OpenID Connect sign-out request.
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.authority, new NaiveSessionCache(userObjectID));
                authContext.TokenCache.Clear();
            }
            
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}