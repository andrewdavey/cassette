using System;
using System.IO;
using System.Web;
using Knapsack.CoffeeScript;
using Knapsack.Utilities;

namespace Knapsack.Integration.Web
{
    /// <summary>
    /// This handler can return javascript module requests and compile CoffeeScript files into JavaScript.
    /// </summary>
    public class KnapsackHttpHandler : IHttpHandler
    {
        readonly ModuleContainer moduleContainer;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public KnapsackHttpHandler() : this(
            KnapsackHttpModule.Manager.ModuleContainer, 
            KnapsackHttpModule.Manager.CoffeeScriptCompiler
        )
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
            var pathInfo = context.Request.PathInfo.Split('/');
            // PathInfo starts with a '/', so first string in array is empty string.
            // Second string will determine what kind of request this is.

            if (pathInfo.Length < 2) 
            {
                BadRequest(context); 
            } 
            else if (pathInfo[1] == "modules")
            {
                ProcessModuleRequest(context);
            }
            else if (pathInfo[1] == "coffee")
            {
                ProcessCoffeeRequest(context);
            }
            else
            {
                BadRequest(context);
            }
        }

        void BadRequest(HttpContext context)
        {
            context.Response.StatusCode = 400;
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
            // Request PathInfo contains the coffeescript file.
            // (After we remove the "/coffee" prefix.)
            var appRelativePath = "~" + context.Request.PathInfo.Substring("/coffee".Length) + ".coffee";
            var path = context.Server.MapPath(appRelativePath);
            
            if (!File.Exists(path))
            {
                context.Response.StatusCode = 404;
                return;
            }

            string javaScript;
            try
            {
                javaScript = coffeeScriptCompiler.CompileFile(path);
            }
            catch (CompileException ex)
            {
                // Since we treat this type of request as "debug" only,
                // return javascript that "alerts" the compilation error.
                // I think it's good to "fail loud"!
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
