using System.Web;
using System.Web.Routing;
using TinyIoC;

namespace Cassette.Aspnet
{
    public class CassetteRouteHandler<T> : IRouteHandler
        where T : class, IHttpHandler
    {
        readonly TinyIoCContainer container;

        public CassetteRouteHandler(TinyIoCContainer container)
        {
            this.container = container;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            using (var requestContainer = container.GetChildContainer())
            {
                requestContainer.Register(requestContext);
                var handler = requestContainer.Resolve<T>();
                return handler;
            }
        }
    }
}
