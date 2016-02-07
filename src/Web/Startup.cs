using System.Web.Http;

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Web.Startup))]

namespace Web
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            OAuth2Config.Configuration(app);
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}