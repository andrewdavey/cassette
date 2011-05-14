using System;
using System.IO;
using System.Web;
using Knapsack.CoffeeScript;
using Knapsack.Utilities;

namespace Knapsack.Web
{
    /// <summary>
    /// This handler can return javascript module requests and compile CoffeeScript files into JavaScript.
    /// </summary>
    public class KnapsackHttpHandler : IHttpHandler
    {
        readonly ModuleContainer scriptModuleContainer;
        readonly ModuleContainer stylesheetModuleContainer;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public KnapsackHttpHandler() : this(
            KnapsackHttpModule.Manager.ScriptModuleContainer,
            KnapsackHttpModule.Manager.StylesheetModuleContainer,
            KnapsackHttpModule.Manager.CoffeeScriptCompiler
        )
        {
        }

        public KnapsackHttpHandler(ModuleContainer scriptModuleContainer, ModuleContainer stylesheetModuleContainer, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.scriptModuleContainer = scriptModuleContainer;
            this.stylesheetModuleContainer = stylesheetModuleContainer;
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public bool IsReusable
        {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            var contextWrapper = new HttpContextWrapper(context);
            ProcessRequest(contextWrapper);
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var pathInfo = context.Request.PathInfo.Split('/');
            // PathInfo starts with a '/', so first string in array is empty string.
            // Second string will determine what kind of request this is.

            if (pathInfo.Length < 2) 
            {
                BadRequest(context); 
            } 
            else if (pathInfo[1] == "scripts")
            {
                ProcessScriptModuleRequest(context);
            }
            else if (pathInfo[1] == "styles")
            {
                ProcessStylesheetModuleRequest(context);
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

        void BadRequest(HttpContextBase context)
        {
            context.Response.StatusCode = 400;
        }

        void NotFound(HttpContextBase context)
        {
            context.Response.StatusCode = 404;
        }

        void NotModified(HttpContextBase context)
        {
            context.Response.StatusCode = 304;
        }

        void ProcessScriptModuleRequest(HttpContextBase context)
        {
            ProcessModuleRequest(context, scriptModuleContainer, "text/javascript");
        }

        void ProcessStylesheetModuleRequest(HttpContextBase context)
        {
            ProcessModuleRequest(context, stylesheetModuleContainer, "text/css");
        }

        void ProcessModuleRequest(HttpContextBase context, ModuleContainer container, string contentType)
        {
            var module = FindModule(context.Request, container);

            if (module == null)
            {
                NotFound(context);
                return;
            }

            var serverETag = module.Hash.ToHexString();
            if (ClientHasCurrentVersion(context.Request, serverETag))
            {
                NotModified(context);
            }
            else
            {
                context.Response.ContentType = contentType;
                SetLongLivedCacheHeaders(context.Response.Cache, serverETag);
                // NOTE: If people want compression then tell IIS to do it using config!
                WriteModuleContentToResponse(module, context.Response);
            }
        }

        void ProcessCoffeeRequest(HttpContextBase context)
        {
            // Request PathInfo contains the coffeescript file.
            // (After we remove the "/coffee" prefix.)
            var appRelativePath = "~" + context.Request.PathInfo.Substring("/coffee".Length) + ".coffee";
            var path = context.Server.MapPath(appRelativePath);
            
            if (!File.Exists(path))
            {
                NotFound(context);
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

        Module FindModule(HttpRequestBase request, ModuleContainer container)
        {
            var modulePath = GetModulePath(request);
            var module = container.FindModule(modulePath);
            return module;
        }

        string GetModulePath(HttpRequestBase request)
        {
            // Path info looks like "/{scripts-or-styles}/module-a/foo_hash".
            var path = request.PathInfo;
            // We want "module-a/foo".
            var indexOfSecondSlash = path.IndexOf('/', 1);
            var prefixLength = indexOfSecondSlash + 1;
            var index = path.LastIndexOf('_');
            if (index >= 0)
            {
                // Remove the prefix and hash suffix
                return path.Substring(prefixLength, index - prefixLength);
            }
            else
            {
                // No hash suffix.
                // Just remove the prefix.
                return path.Substring(prefixLength);
            }
        }

        bool ClientHasCurrentVersion(HttpRequestBase request, string serverETag)
        {
            var clientETag = request.Headers["If-None-Match"];
            return clientETag == serverETag;
        }

        void SetLongLivedCacheHeaders(HttpCachePolicyBase cache, string serverETag)
        {
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetETag(serverETag);
            cache.SetExpires(DateTime.UtcNow.AddYears(1));
        }

        void WriteModuleContentToResponse(Module module, HttpResponseBase response)
        {
            using (var stream = scriptModuleContainer.OpenModuleFile(module))
            {
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}
