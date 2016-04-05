using System;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace Web.Middleware
{
    public sealed class RequestSchemeMiddleware : OwinMiddleware
    {
        public RequestSchemeMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            if (string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Request.Scheme = "https";
            }

            return Next.Invoke(context);
        }
    }
}