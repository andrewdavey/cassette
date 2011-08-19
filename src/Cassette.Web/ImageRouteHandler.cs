using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class ImageRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ImageRequestHandler(requestContext);
        }
    }
}