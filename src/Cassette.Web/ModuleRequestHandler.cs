using System;
using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class ModuleRequestHandler<T> : IHttpHandler
        where T : Module
    {
        public ModuleRequestHandler(IModuleContainer<T> moduleContainer, RequestContext requestContext)
        {
            this.moduleContainer = moduleContainer;
            this.requestContext = requestContext;
        }

        readonly IModuleContainer<T> moduleContainer;
        readonly RequestContext requestContext;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = requestContext.RouteData.GetRequiredString("path");
            var index = path.LastIndexOf('_');
            if (index >= 0)
            {
                path = path.Substring(0, index);
            }
            var module = moduleContainer.FindModuleByPath(path);
            var response = requestContext.HttpContext.Response;
            if (module == null)
            {
                response.StatusCode = 404;
            }
            else
            {
                // TODO: Check for E-Tag and If-Modified-Since headers.

                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetMaxAge(TimeSpan.FromDays(365));
                response.ContentType = module.ContentType;
                using (var assetStream = module.Assets[0].OpenStream())
                {
                    assetStream.CopyTo(response.OutputStream);
                }
            }
        }
    }
}
