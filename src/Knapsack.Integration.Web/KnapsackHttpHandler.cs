using System;
using System.IO;
using System.Web;
using Knapsack.CoffeeScript;

namespace Knapsack.Integration.Web
{
    public class KnapsackHttpHandler : IHttpHandler
    {
        readonly ModuleContainer moduleContainer;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public KnapsackHttpHandler()
            : this(KnapsackHttpModule.Instance.ModuleContainer, KnapsackHttpModule.Instance.CoffeeScriptCompiler)
        {
        }

        public KnapsackHttpHandler(ModuleContainer moduleContainer, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.moduleContainer = moduleContainer;
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // PathInfo starts with a '/'
            var pathInfo = context.Request.PathInfo.Split('/');
            if (pathInfo.Length < 2) return;
            if (pathInfo[1] == "modules")
            {
                ProcessModuleRequest(context);
            }
            else if (pathInfo[1] == "coffee")
            {
                ProcessCoffeeRequest(context);
            }
        }

        void ProcessModuleRequest(HttpContext context)
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

        void ProcessCoffeeRequest(HttpContext context)
        {
            var path = context.Server.MapPath("~" + context.Request.PathInfo.Substring("/coffee".Length) + ".coffee");
            string javaScript;

            try
            {
                javaScript = coffeeScriptCompiler.CompileFile(path);
            }
            catch (CoffeeScript.CompileException ex)
            {
                javaScript = JavaScriptErrorAlert(ex);
            }

            context.Response.ContentType = "text/javascript";
            context.Response.Write(javaScript);
        }

        string JavaScriptErrorAlert(CompileException ex)
        {
            return "alert('CoffeeScript compile error in "
                    + JavaScriptUtilities.EscapeJavaScriptString(ex.SourcePath)
                    + "\\r\\n"
                    + JavaScriptUtilities.EscapeJavaScriptString(ex.Message)
                    + "');";
        }

        Module FindModule(HttpRequest request)
        {
            var modulePath = GetModulePath(request);
            var module = moduleContainer.FindModule(modulePath);
            return module;
        }

        string GetModulePath(HttpRequest request)
        {
            // Path info looks like "/modules/module-a/foo_hash".
            var path = request.PathInfo;
            // We want "module-a/foo".
            var prefixLength = "/modules/".Length;
            return path.Substring(prefixLength, path.LastIndexOf('_') - prefixLength);
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
            using (var stream = moduleContainer.OpenModuleFile(module))
            {
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}
