using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knapsack.Utilities;

namespace Knapsack.Web
{
    /// <summary>
    /// Enables a page to reference client scripts and stylesheets.
    /// The required script and link elements can then be generated.
    /// </summary>
    public class PageHelper : IPageHelper
    {
        readonly bool useModules;
        readonly bool bufferHtmlOutput;
        readonly IReferenceBuilder scriptReferenceBuilder;
        readonly IReferenceBuilder stylesheetReferenceBuilder;
        readonly Func<string, string> virtualPathToAbsolute;
        readonly string stylesheetsPlaceholder;

        public PageHelper(bool useModules, bool bufferHtmlOutput, IReferenceBuilder scriptReferenceBuilder, IReferenceBuilder stylesheetReferenceBuilder, Func<string, string> virtualPathToAbsolute)
        {
            this.useModules = useModules;
            this.bufferHtmlOutput = bufferHtmlOutput;
            this.scriptReferenceBuilder = scriptReferenceBuilder;
            this.stylesheetReferenceBuilder = stylesheetReferenceBuilder;
            this.virtualPathToAbsolute = virtualPathToAbsolute;

            if (bufferHtmlOutput)
            {
                stylesheetsPlaceholder = "$Knapsack-Stylesheets-" + Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Records that the calling view requires the given script path. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPath">The application relative path to the script file.</param>
        public void ReferenceScript(string scriptPath)
        {
            scriptReferenceBuilder.AddReference(scriptPath);
        }

        /// <summary>
        /// Records that the calling view requires the given stylesheet. This does not render any
        /// HTML. Call <see cref="RenderStylesheets"/> to actually output the link elements.
        /// </summary>
        /// <param name="stylesheetPath">The application relative path to the stylesheet file.</param>
        public void ReferenceStylesheet(string stylesheetPath)
        {
            stylesheetReferenceBuilder.AddReference(stylesheetPath);
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
                .Select(virtualPathToAbsolute)
                .Select(HttpUtility.HtmlAttributeEncode)
                .Select(src => string.Format(template, HttpUtility.HtmlAttributeEncode(src)));
            
            var allHtml = string.Join("\r\n", scriptElements);
            return new HtmlString(allHtml);
        }

        public string StylesheetsPlaceholder
        {
            get
            {
                return stylesheetsPlaceholder;
            }
        }

        /// <summary>
        /// Creates HTML link elements for all required stylesheets and their dependencies.
        /// </summary>
        public IHtmlString RenderStylesheetLinks()
        {
            if (bufferHtmlOutput)
            {
                // Only output a placeholder for now. The BufferStream will insert the links later,
                // to give partial views a chance to add stylesheet references.
                return new HtmlString(
                    Environment.NewLine +
                    StylesheetsPlaceholder +
                    Environment.NewLine
                );
            }
            else
            {
                return new HtmlString(GetStylesheetLinks());
            }
        }

        public string GetStylesheetLinks()
        {
            var cssUrls = useModules
                ? ReleaseStylesheetUrls()
                : DebugStylesheetUrls();

            var template = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
            var linkElements = cssUrls
                .Select(virtualPathToAbsolute)
                .Select(HttpUtility.HtmlAttributeEncode)
                .Select(src => string.Format(template, HttpUtility.HtmlAttributeEncode(src)));

            var allHtml = string.Join("\r\n", linkElements);
            return allHtml;
        }

        IEnumerable<string> DebugScriptUrls()
        {
            return scriptReferenceBuilder
                .GetRequiredModules()
                .SelectMany(m => m.Resources)
                .Select(r => new { url = AppRelativeScriptUrl(r), hash = r.Hash.ToHexString() })
                .Select(r => r.url + (r.url.Contains('?') ? "&" : "?") + r.hash);
        }

        string AppRelativeScriptUrl(Resource script)
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
        /// Returns the application relative URL for each required script module.
        /// The hash of each module is appended to enable long-lived caching that
        /// is broken when a module changes.
        /// </summary>
        IEnumerable<string> ReleaseScriptUrls()
        {
            return scriptReferenceBuilder
                .GetRequiredModules()
                .Select(m => "~/knapsack.axd/scripts/" + m.Path + "_" + m.Hash.ToHexString());
        }

        IEnumerable<string> DebugStylesheetUrls()
        {
            return stylesheetReferenceBuilder
                .GetRequiredModules()
                .SelectMany(m => m.Resources)
                .Select(r => new { url = AppRelativeStylesheetUrl(r), hash = r.Hash.ToHexString() })
                .Select(r => r.url + (r.url.Contains('?') ? "&" : "?") + r.hash);
        }

        /// <summary>
        /// Returns the application relative URL for each required stylesheet module.
        /// The hash of each module is appended to enable long-lived caching that
        /// is broken when a module changes.
        /// </summary>
        IEnumerable<string> ReleaseStylesheetUrls()
        {
            return stylesheetReferenceBuilder
                .GetRequiredModules()
                .Select(m => "~/knapsack.axd/styles/" + m.Path + "_" + m.Hash.ToHexString());
        }

        string AppRelativeStylesheetUrl(Resource stylesheet)
        {
            // TODO: Check for .less and .sass files
            return "~/" + stylesheet.Path;
        }
    }
}
