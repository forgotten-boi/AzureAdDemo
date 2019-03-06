using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using PocAadClient.Models;
using PocAadClient.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PocAadClient.Controllers
{
    [Authorize]
    public class WeatherController : Controller
    {

        private string todoListResourceId = ConfigurationManager.AppSettings["todo:TodoListResourceId"];
        private string todoListBaseAddress = ConfigurationManager.AppSettings["todo:TodoListBaseAddress"];
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:Password"];

        // GET: Weather
        public async Task<ActionResult> Index()
        {
            //AuthenticationResult result = null;
            List<Weather> itemList = new List<Weather>();

            try
            {
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                //AuthenticationContext authContext = new AuthenticationContext(Startup.authority, new NaiveSessionCache(userObjectID));
                AuthenticationContext authContext = new AuthenticationContext(Startup.authority, false);
                ClientCredential credential = new ClientCredential(clientId, appKey);

                //AuthenticationResult authenticationResult = await authContext.AcquireTokenAsync(clientId, credential);


                var result = await authContext.AcquireTokenSilentAsync(todoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

           

                //
                // Retrieve the user's To Do List.
                //

                ServicePointManager.ServerCertificateValidationCallback += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    return true;
                };



                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, todoListBaseAddress + "api/Values");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                HttpResponseMessage response = await client.SendAsync(request);

                //
                // Return the To Do List in the view.
                //
                if (response.IsSuccessStatusCode)
                {
                    List<Dictionary<String, String>> responseElements = new List<Dictionary<String, String>>();
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    String responseString = await response.Content.ReadAsStringAsync();
                    responseElements = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(responseString, settings);
                    foreach (Dictionary<String, String> responseElement in responseElements)
                    {
                        Weather newItem = new Weather();
                        newItem.Summary = responseElement["Summary"];
                        newItem.TemperatureC = !string.IsNullOrEmpty(responseElement["TemperatureC"]) ?Convert.ToInt32(responseElement["TemperatureC"]):default(int);
                        newItem.DateFormatted = responseElement["DateFormatted"];
                        itemList.Add(newItem);
                    }

                    return View(itemList);
                }
                else
                {
                    //
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    //     and show the user an error indicating they might need to sign-in again.
                    //
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Resource == todoListResourceId);
                        foreach (TokenCacheItem tci in todoTokens)
                            authContext.TokenCache.DeleteItem(tci);

                        ViewBag.ErrorMessage = "UnexpectedError";
                        Weather newItem = new Weather();
                        newItem.Summary = "(No items in list)";
                        itemList.Add(newItem);
                        return View(itemList);
                    }
                }
            }
            catch (AdalException ee)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext().Authentication.Challenge(
                        new AuthenticationProperties(),
                        OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                Weather newItem = new Weather();
                newItem.Summary = "(Sign-in required to view to do list.)";
                itemList.Add(newItem);
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(itemList);
            }


            //
            // If the call failed for any other reason, show the user an error.
            //
            return View("Error");
        }
    }
}