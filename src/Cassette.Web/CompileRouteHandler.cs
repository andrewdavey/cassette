using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class CompileRouteHandler : IRouteHandler
    {
        public CompileRouteHandler(CassetteApplication application)
        {
            this.application = application;
        }

        readonly CassetteApplication application;

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new CompileRequestHandler(requestContext, application.GetCompiler);
        }
    }
}
