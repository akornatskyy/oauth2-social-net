using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

using Microsoft.Owin.Security;

using Web.Constants;

namespace Web.Controllers
{
    [Authorize, Route(RoutePatterns.Token, Name = RouteNames.Token)]
    public sealed class TokenController : ApiController
    {
        public IHttpActionResult Get(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                this.ModelState.AddModelError("returnUrl", Properties.Resources.Required);
                return this.BadRequest(this.ModelState);
            }

            var email = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Email);
            if (email == null)
            {
                this.ModelState.AddModelError("email", Properties.Resources.Required);
                return this.BadRequest(this.ModelState);
            }

            return this.IssueChallenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        public IHttpActionResult Post()
        {
            return this.IssueChallenge(new AuthenticationProperties() { AllowRefresh = true });
        }

        private IHttpActionResult IssueChallenge(AuthenticationProperties props)
        {           
            this.Request.GetOwinContext().Authentication.Challenge(
                props, 
                OAuth2Config.OAuthBearerOptions.AuthenticationType);
            return this.Unauthorized();
        }
    }
}