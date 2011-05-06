using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Knapsack.Integration.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        public static void AddScriptReference(this HtmlHelper html, string scriptPath)
        {
            var builder = GetReferenceBuilder(html);
            builder.AddReference(scriptPath);
        }

        public static IHtmlString RenderScripts(this HtmlHelper html)
        {
            var builder = GetReferenceBuilder(html);
            var scriptUrls = html.ViewContext.HttpContext.IsDebuggingEnabled
                ? DebugScriptUrls(builder) 
                : ReleaseScriptUrls(builder);

            var template = "<script src=\"{0}\" type=\"text/javascript\"></script>";
            var scriptElements = scriptUrls
                .Select(VirtualPathUtility.ToAbsolute)
                .Select(HttpUtility.HtmlAttributeEncode)
                .Select(src => string.Format(template, src));

            var allHtml = string.Join("\r\n", scriptElements);
            return new HtmlString(allHtml);
        }

        static ReferenceBuilder GetReferenceBuilder(HtmlHelper html)
        {
            var builder = (ReferenceBuilder)html.ViewContext.HttpContext.Items["Knapsack.ReferenceBuilder"];
            if (builder == null)
            {
                throw new InvalidOperationException("Knapsack.ReferenceBuilder has not been added to the current HttpContext Items.");
            }
            return builder;
        }

        static IEnumerable<string> DebugScriptUrls(ReferenceBuilder builder)
        {
            var cacheBreaker = "nocache=" + DateTime.Now.Ticks.ToString();
            return builder.GetRequiredModules()
                .SelectMany(m => m.Scripts)
                .Select(s => {
                    if (s.Path.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase))
                    {
                        return CoffeeScriptUrl(s.Path);
                    }
                    else
                    {
                        return "~/" + s.Path;
                    }
                })
                .Select(url => url + (url.Contains('?') ? "&" : "?") + cacheBreaker);
        }

        static string CoffeeScriptUrl(string path)
        {
            // Must remove the file extension from the path, 
            // otherwise the knapsack.axd handler is not called.
            // I guess asp.net favours the last file extension it finds.
            var pathWithoutExtension = path.Substring(0, path.Length - ".coffee".Length);
            // Return the URL that will invoke the CoffeeScript compiler to return JavaScript.
            return "~/knapsack.axd/coffee/" + pathWithoutExtension;
        }

        /// <summary>
        /// Returns the application relative URL for each required module.
        /// The hash of each module is appended to enable long-lived caching that
        /// is broken when a module changes.
        /// </summary>
        static IEnumerable<string> ReleaseScriptUrls(ReferenceBuilder builder)
        {
            return builder.GetRequiredModules()
                 .Select(m => "~/knapsack.axd/modules/" + m.Path + "_" + m.Hash.ToHexString());
        }
    }
}
