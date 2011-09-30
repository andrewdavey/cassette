using System;
using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class DelegateRouteHandler : IRouteHandler
    {
        readonly Func<RequestContext, IHttpHandler> createHandler;

        public DelegateRouteHandler(Func<RequestContext, IHttpHandler> createHandler)
        {
            this.createHandler = createHandler;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return createHandler(requestContext);
        }
    }
}