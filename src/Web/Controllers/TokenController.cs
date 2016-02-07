using System;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace Web.Controllers
{
    public sealed class TokenController : ApiController
    {
        [Authorize, Route("token")]
        public IHttpActionResult Get(string returnUrl)
        {
            Request.GetOwinContext().Authentication.SignOut();
            var email = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Email);
            if (email == null)
            {
                return this.BadRequest();
            }

            var expires = 7200;
            ClaimsIdentity identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, email.Value));
            var props = new AuthenticationProperties()
            {
                ExpiresUtc = DateTime.UtcNow.AddSeconds(expires)
            };

            var ticket = new AuthenticationTicket(identity, props);
            var token = OAuth2Config.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
            var uri = new UriBuilder(returnUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);
            query.Add("token", token);
            query.Add("expires", expires.ToString());
            uri.Query = query.ToString();
            return this.Redirect(uri.ToString());
        }
    }
}