using System;
using System.Globalization;
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
            b.Path = "/token";
            this.Request.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties() { RedirectUri = b.ToString() },
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(provider));
            return this.Unauthorized();
        }
    }
}