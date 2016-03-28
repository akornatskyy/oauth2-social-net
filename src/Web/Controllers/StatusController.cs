using System.Diagnostics;
using System.Security.Claims;
using System.Web.Http;

using Web.Constants;

namespace Web.Controllers
{
    [Authorize, Route(RoutePatterns.Status)]
    public sealed class StatusController : ApiController
    {
        [HttpGet, HttpPost]
        public object Index()
        {
            var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Email);
            Trace.Assert(claim != null);
            return new { email = claim.Value };
        }
    }
}