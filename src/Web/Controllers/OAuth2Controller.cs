using System;
using System.Net.Http;
using System.Web.Http;

using Microsoft.Owin.Security;

using Web.Constants;

namespace Web.Controllers
{
    [Route(RoutePatterns.OAuth2)]
    public sealed class OAuth2Controller : ApiController
    {
        public IHttpActionResult Get(string provider)
        {
            var b = new UriBuilder(Request.RequestUri)
            {
                Path = Url.Route(RouteNames.Token, null)
            };
            this.Request.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties() { RedirectUri = b.ToString() },
                provider.ToLowerInvariant());
            return this.Unauthorized();
        }
    }
}