using System.Web.Http;

using Microsoft.Owin;
using Owin;

using Web.Middleware;

[assembly: OwinStartupAttribute(typeof(Web.Startup))]

namespace Web
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(RequestSchemeMiddleware));
            OAuth2Config.Configuration(app);
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}