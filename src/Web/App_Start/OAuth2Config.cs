using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

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
        internal static readonly OAuthBearerAuthenticationOptions OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

        private static readonly HttpClient FacebookHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://graph.facebook.com/me?fields=email"),
            MaxResponseContentBufferSize = 256
        };

        private static readonly double TokenExpire = TimeSpan.Parse(
            ConfigurationManager.AppSettings.Get("Token.ExpireTimeSpan")).TotalSeconds;

        public static void Configuration(IAppBuilder app)
        {
            var c = ConfigureCookieAuthentication();
            app.UseCookieAuthentication(c);
            app.SetDefaultSignInAsAuthenticationType(c.AuthenticationType);
            app.UseOAuthBearerAuthentication(ConfigureBearerAuthentication());            
            app.UseGoogleAuthentication(ConfigureGoogleAuthentication());
            app.UseFacebookAuthentication(ConfigureFacebookAuthentication());            
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);            
        }

        #region Configure Cookie Authentication

        private static CookieAuthenticationOptions ConfigureCookieAuthentication()
        {
            return new CookieAuthenticationOptions
            {                
                CookieHttpOnly = true,
                ExpireTimeSpan = TimeSpan.FromSeconds(30)
            };
        }

        #endregion

        #region Configure Bearer Authentication

        private static OAuthBearerAuthenticationOptions ConfigureBearerAuthentication()
        {
            OAuthBearerOptions.Provider = new OAuthBearerAuthenticationProvider
            {
                OnApplyChallenge = ctx =>
                {
                    var auth = ctx.OwinContext.Authentication;
                    if (auth.AuthenticationResponseChallenge == null)
                    {
                        return Task.FromResult(true);
                    }

                    var claim = auth.User.FindFirst(ClaimTypes.Email);
                    if (claim == null)
                    {
                        return Task.FromResult(false);
                    }

                    var options = OAuth2Config.OAuthBearerOptions;
                    var token = options.AccessTokenFormat.Protect(new AuthenticationTicket(
                        new ClaimsIdentity(new[] { claim }, options.AuthenticationType),
                        new AuthenticationProperties() { ExpiresUtc = options.SystemClock.UtcNow.AddSeconds(OAuth2Config.TokenExpire) }));

                    var props = auth.AuthenticationResponseChallenge.Properties;
                    var returnUrl = props.RedirectUri;
                    if (returnUrl != null)
                    {
                        var uri = new UriBuilder(new Uri(ctx.Request.Uri, returnUrl));
                        var query = HttpUtility.ParseQueryString(uri.Query);
                        query.Add("token", token);
                        query.Add("expires", OAuth2Config.TokenExpire.ToString(CultureInfo.InvariantCulture));
                        uri.Query = query.ToString();

                        auth.SignOut();
                        ctx.Response.Redirect(uri.ToString());
                        return Task.FromResult(true);
                    }

                    if (props.AllowRefresh.GetValueOrDefault(false))
                    {
                        ctx.Response.StatusCode = 201;
                        ctx.Response.ContentType = "application/json";
                        var content = string.Format("{{\"token\":\"{0}\",\"expires\":{1}}}", token, OAuth2Config.TokenExpire);
                        ctx.Response.ContentLength = content.Length;
                        ctx.Response.Write(content);
                    }

                    return Task.FromResult(true);
                }
            };
            return OAuthBearerOptions;
        }

        #endregion

        #region Configure Google Authentication

        private static GoogleOAuth2AuthenticationOptions ConfigureGoogleAuthentication()
        {
            var settings = ConfigurationManager.AppSettings;
            var options = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "google",
                ClientId = settings.Get("Google.ClientId"),
                ClientSecret = settings.Get("Google.ClientSecret"),
                CallbackPath = new PathString("/oauth2/google/callback")
            };
            options.Scope.Clear();
            options.Scope.Add("email");
            options.Provider = new GoogleOAuth2AuthenticationProvider
            {
                OnAuthenticated = ctx =>
                {
                    CleanUpAllClaimsButEmail(ctx.Identity);
                    return Task.FromResult(true);
                }
            };

            return options;
        }

        #endregion

        #region Configure Facebook Authentication

        private static FacebookAuthenticationOptions ConfigureFacebookAuthentication()
        {
            var settings = ConfigurationManager.AppSettings;
            var options = new FacebookAuthenticationOptions
            {
                AuthenticationType = "facebook",
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
                    CleanUpAllClaimsButEmail(ctx.Identity);
                    if (ctx.Email != null)
                    {
                        return;
                    }

                    var email = await FindEmailInFacebook(ctx.AccessToken);
                    if (email == null)
                    {
                        return;
                    }

                    ctx.Identity.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            };
            
            return options;
        }

        private static async Task<string> FindEmailInFacebook(string accessToken)
        {
            var response = await FacebookHttpClient.SendAsync(new HttpRequestMessage
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
                }
            });
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var d = JObject.Parse(await response.Content.ReadAsStringAsync());
            return (string)d["email"];
        }

        #endregion

        private static void CleanUpAllClaimsButEmail(ClaimsIdentity identity)
        {
            foreach (var c in identity.Claims.Where(c => c.Type != ClaimTypes.Email).ToArray())
            {
                identity.RemoveClaim(c);
            }
        }
    }
}