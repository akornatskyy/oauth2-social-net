using System.Web.Http;

namespace Web.Controllers
{
    [Authorize]
    [Route("status")]
    public sealed class StatusController : ApiController
    {
        public object Get()
        {
            return new { name = User.Identity.Name };
        }
    }
}