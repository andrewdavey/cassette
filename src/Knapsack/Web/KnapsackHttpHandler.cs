using System;
using System.Web;
using System.Web.Caching;

namespace Knapsack.Web
{
    public class KnapsackHttpHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var module = FindModule(context.Request);

            if (module == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var serverETag = module.Hash.ToHexString();
            if (ClientHasCurrentVersion(context.Request, serverETag))
            {
                context.Response.StatusCode = 304; // Not Modified;
            }
            else
            {
                context.Response.ContentType = "text/javascript";
                SetLongLivedCacheHeaders(context.Response.Cache, serverETag);
                // NOTE: If people want compression then tell IIS to do it using config!
                WriteModuleContentToResponse(module, context.Response);
            }
        }

        Module FindModule(HttpRequest request)
        {
            var modulePath = GetModulePath(request);
            var module = KnapsackHttpModule.Instance.ModuleContainer.FindModule(modulePath);
            return module;
        }

        string GetModulePath(HttpRequest request)
        {
            // Path info looks like "/module-a/foo_hash".
            var path = request.PathInfo;
            // We want "module-a/foo".
            return path.Substring(1, path.LastIndexOf('_') - 1);
        }

        bool ClientHasCurrentVersion(HttpRequest request, string serverETag)
        {
            var clientETag = request.Headers["If-None-Match"];
            return clientETag == serverETag;
        }

        void SetLongLivedCacheHeaders(HttpCachePolicy cache, string serverETag)
        {
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetETag(serverETag);
            cache.SetExpires(DateTime.UtcNow.AddYears(1));
        }

        void WriteModuleContentToResponse(Module module, HttpResponse response)
        {
            using (var stream = KnapsackHttpModule.Instance.ModuleContainer.OpenModuleFile(module))
            {
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}
