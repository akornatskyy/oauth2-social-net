using System;
using System.Net.Http;
using System.Web.Http;

using Microsoft.Owin.Security;

namespace Web.Controllers
{
    [Route("oauth2/{provider}")]
    public sealed class OAuth2Controller : ApiController
    {
        public IHttpActionResult Get(string provider)
        {
            var b = new UriBuilder(Request.RequestUri);
            b.Path = Url.Route("token", null);
            this.Request.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties() { RedirectUri = b.ToString() },
                provider.ToLowerInvariant());
            return this.Unauthorized();
        }
    }
}