using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knapsack.Utilities;

namespace Knapsack.Integration.Web
{
    /// <summary>
    /// Enables a page to reference client scripts and generates all the required script elements.
    /// </summary>
    public class PageHelper
    {
        readonly ReferenceBuilder referenceBuilder;
        readonly bool useModules;

        public PageHelper(bool useModules, ReferenceBuilder referenceBuilder)
        {
            this.useModules = useModules;
            this.referenceBuilder = referenceBuilder;
        }

        /// <summary>
        /// Records that the calling view requires the given script path. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPath">The application relative path to the script file.</param>
        public void AddScriptReference(string scriptPath)
        {
            referenceBuilder.AddReference(scriptPath);
        }

        /// <summary>
        /// Creates HTML script elements for all required scripts and their dependencies.
        /// </summary>
        public IHtmlString RenderScripts()
        {
            var scriptUrls = useModules
                ? ReleaseScriptUrls()
                : DebugScriptUrls();

            var template = "<script src=\"{0}\" type=\"text/javascript\"></script>";
            var scriptElements = scriptUrls
                .Select(VirtualPathUtility.ToAbsolute)
                .Select(HttpUtility.HtmlAttributeEncode)
                .Select(src => string.Format(template, HttpUtility.HtmlAttributeEncode(src)));
            
            var allHtml = string.Join("\r\n", scriptElements);
            return new HtmlString(allHtml);
        }

        IEnumerable<string> DebugScriptUrls()
        {
            var cacheBreaker = "nocache=" + DateTime.Now.Ticks.ToString();
            return referenceBuilder
                .GetRequiredModules()
                .SelectMany(m => m.Scripts)
                .Select(AppRelativeScriptUrl)
                .Select(url => url + (url.Contains('?') ? "&" : "?") + cacheBreaker);
        }

        string AppRelativeScriptUrl(Script script)
        {
            if (script.Path.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase))
            {
                return CoffeeScriptUrl(script.Path);
            }
            else
            {
                return "~/" + script.Path;
            }
        }

        string CoffeeScriptUrl(string path)
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
        IEnumerable<string> ReleaseScriptUrls()
        {
            return referenceBuilder
                .GetRequiredModules()
                .Select(m => "~/knapsack.axd/modules/" + m.Path + "_" + m.Hash.ToHexString());
        }
    }
}
