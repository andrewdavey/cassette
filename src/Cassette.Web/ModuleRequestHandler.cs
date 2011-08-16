using System;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;

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

        // TODO: Explicit implement this
        public void ProcessRequest(HttpContext _)
        {
            ProcessRequest();
        }

        public void ProcessRequest()
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
                var actualETag = module.Assets[0].Hash.ToHexString();
                var givenETag = requestContext.HttpContext.Request.Headers["If-None-Match"];
                if (givenETag == actualETag)
                {
                    response.StatusCode = 304; // Not Modified
                    response.SuppressContent = true;
                }
                else
                {
                    response.Cache.SetCacheability(HttpCacheability.Public);
                    response.Cache.SetMaxAge(TimeSpan.FromDays(365));
                    response.Cache.SetETag(actualETag);
                    response.ContentType = module.ContentType;
                    using (var assetStream = module.Assets[0].OpenStream())
                    {
                        assetStream.CopyTo(response.OutputStream);
                    }
                }
            }
        }
    }
}
