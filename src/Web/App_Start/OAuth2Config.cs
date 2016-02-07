using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;

using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using Owin;

namespace Web
{
    public static class OAuth2Config
    {
        public static readonly OAuthBearerAuthenticationOptions OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

        public static void Configuration(IAppBuilder app)
        {
            var c = new CookieAuthenticationOptions 
            {
                 CookieHttpOnly = true,
                 ExpireTimeSpan = TimeSpan.FromSeconds(30)
            };
            app.UseCookieAuthentication(c);
            app.SetDefaultSignInAsAuthenticationType(c.AuthenticationType);
            app.UseGoogleAuthentication(ConfigureGoogleAuthentication());
            app.UseFacebookAuthentication(ConfigureFacebookAuthentication());
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);            
        }

        private static GoogleOAuth2AuthenticationOptions ConfigureGoogleAuthentication()
        {
            var settings = ConfigurationManager.AppSettings;
            var options = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = settings.Get("Google.ClientId"),
                ClientSecret = settings.Get("Google.ClientSecret"),
                CallbackPath = new PathString("/oauth2/google/callback")
            };
            options.Scope.Clear();
            options.Scope.Add("email");
            return options;
        }

        private static FacebookAuthenticationOptions ConfigureFacebookAuthentication()
        {
            var settings = ConfigurationManager.AppSettings;
            var options = new FacebookAuthenticationOptions
            {
                AppId = settings.Get("Facebook.AppId"),
                AppSecret = settings.Get("Facebook.AppSecret"),
                CallbackPath = new PathString("/oauth2/facebook/callback")
            };
            options.Scope.Clear();
            options.Scope.Add("email");
            options.Provider = new FacebookAuthenticationProvider
            {
                OnAuthenticated = async ctx =>
                {
                    if (ctx.Email != null)
                    {
                        return;
                    }

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                        var response = await client.GetAsync(options.UserInformationEndpoint + "?fields=email");
                        if (!response.IsSuccessStatusCode)
                        {
                            return;
                        }

                        var d = JObject.Parse(await response.Content.ReadAsStringAsync());
                        ctx.Identity.AddClaim(new Claim(ClaimTypes.Email, (string)d["email"]));
                    }
                }
            };
            
            return options;
        }
    }
}