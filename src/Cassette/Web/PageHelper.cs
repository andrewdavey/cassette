using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.Utilities;

namespace Cassette.Web
{
    /// <summary>
    /// Enables a page to reference client scripts and stylesheets.
    /// The required script and link elements can then be generated.
    /// </summary>
    public class PageHelper : IPageHelper
    {
        readonly bool useModules;
        readonly bool bufferHtmlOutput;
        readonly string handler;
        readonly IReferenceBuilder scriptReferenceBuilder;
        readonly IReferenceBuilder stylesheetReferenceBuilder;
        readonly Func<string, string> virtualPathToAbsolute;
        readonly string stylesheetsPlaceholder;
        readonly string scriptsPlaceholderPrefix;
        readonly Dictionary<string, string> scriptPlaceholders;

        public PageHelper(bool useModules, bool bufferHtmlOutput, string handler, IReferenceBuilder scriptReferenceBuilder, IReferenceBuilder stylesheetReferenceBuilder, Func<string, string> virtualPathToAbsolute)
        {
            this.useModules = useModules;
            this.bufferHtmlOutput = bufferHtmlOutput;
            this.handler = handler;
            this.scriptReferenceBuilder = scriptReferenceBuilder;
            this.stylesheetReferenceBuilder = stylesheetReferenceBuilder;
            this.virtualPathToAbsolute = virtualPathToAbsolute;

            if (bufferHtmlOutput)
            {
                var unique = Guid.NewGuid().ToString();
                stylesheetsPlaceholder = "$Cassette-Stylesheets-" + unique;
                scriptsPlaceholderPrefix = "$Cassette-Scripts-" + unique + "-";
                scriptPlaceholders = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Records that the calling view requires the given script. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPathOrUrl">The application relative path to the script file or an absolute external script URL.</param>
        public void ReferenceScript(string scriptPathOrUrl)
        {
            scriptReferenceBuilder.AddReference(scriptPathOrUrl);
        }

        /// <summary>
        /// Records that the calling view requires the given script. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPath">The absolute external script URL.</param>
        /// <param name="location">The location identifier for this script e.g. "head" or "body".</param>
        public void ReferenceExternalScript(string externalScriptUrl, string location)
        {
            scriptReferenceBuilder.AddExternalReference(externalScriptUrl, location);
        }

        /// <summary>
        /// Creates HTML script elements for all required scripts (tagged as having the given location) and their dependencies.
        /// When buffering HTML output, a placeholder is returned instead.
        /// </summary>
        /// <param name="location">The location being rendered.</param>
        public IHtmlString RenderScripts(string location)
        {
            // Treat null location as empty string since config seems to create empty strings
            // even though the default value is meant to be null.
            location = location ?? "";

            if (bufferHtmlOutput)
            {
                // Only output a placeholder for now. ReplacePlaceholders is used later to insert
                // the actual HTML. This means partial views can have their references inserted
                // into the <head> even after it's been rendered.
                var placeholder = GetScriptsPlaceholder(location);
                // Keep track of the placeholder we've generated for each location.
                if (!scriptPlaceholders.ContainsKey(placeholder))
                {
                    scriptPlaceholders.Add(placeholder, location);
                }
                return new HtmlString(
                    Environment.NewLine +
                    placeholder +
                    Environment.NewLine
                );
            }
            else
            {
                var html = CreateScriptsHtml(location);
                return new HtmlString(html);
            }
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
        /// Creates HTML link elements for all required stylesheets and their dependencies.
        /// When buffering HTML output, a placeholder is returned instead.
        /// </summary>
        public IHtmlString RenderStylesheetLinks()
        {
            if (bufferHtmlOutput)
            {
                // Only output a placeholder for now. The BufferStream will insert the links later,
                // to give partial views a chance to add stylesheet references.
                return new HtmlString(
                    Environment.NewLine +
                    stylesheetsPlaceholder +
                    Environment.NewLine
                );
            }
            else
            {
                return new HtmlString(CreateStylesheetsHtml());
            }
        }

        /// <summary>
        /// When buffering HTML output, the line is checked for known script and stylesheet placeholders.
        /// They are replaced with the relevant HTML.
        /// </summary>
        /// <param name="line">The line to check for placeholders.</param>
        /// <returns>The line, but with placeholders replaced with their respective HTML.</returns>
        public string ReplacePlaceholders(string line)
        {
            if (!bufferHtmlOutput) throw new InvalidOperationException("Cannot replace placeholders when HTML output buffering is disabled.");

            if (line == null)
            {
                return line;
            }
            else if (line == stylesheetsPlaceholder)
            {
                return CreateStylesheetsHtml();
            }
            else if (line.StartsWith(scriptsPlaceholderPrefix))
            {
                string location;
                if (scriptPlaceholders.TryGetValue(line, out location))
                {
                    return CreateScriptsHtml(location);
                }
            }

            // Otherwise, do not change the line
            return line;
        }

        string GetScriptsPlaceholder(string location)
        {
            return scriptsPlaceholderPrefix + (location ?? "");
        }

        string CreateScriptsHtml(string location)
        {
            var scriptUrls = useModules
                ? ReleaseScriptUrls(location)
                : DebugScriptUrls(location);

            var template = "<script src=\"{0}\" type=\"text/javascript\"></script>";
            var scriptElements = BuildHtmlElements(scriptUrls, template);

            var allHtml = string.Join("\r\n", scriptElements);
            return allHtml;
        }

        string CreateStylesheetsHtml()
        {
            var cssUrls = useModules
                ? ReleaseStylesheetUrls()
                : DebugStylesheetUrls();

            var template = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
            var linkElements = BuildHtmlElements(cssUrls, template);

            var allHtml = string.Join("\r\n", linkElements);
            return allHtml;
        }

        IEnumerable<string> BuildHtmlElements(IEnumerable<string> urls, string template)
        {
            return urls
                .Select(url => url.StartsWith("http:") || url.StartsWith("https:") ? url : virtualPathToAbsolute(url))
                .Select(HttpUtility.HtmlAttributeEncode)
                .Select(src => string.Format(template, src));
        }

        IEnumerable<string> DebugScriptUrls(string location)
        {
            return scriptReferenceBuilder
                .GetRequiredModules()
                .Where(m => m.Location == location)
                .SelectMany(m => m.Resources)
                .Select(DebugScriptUrl);
        }

        string DebugScriptUrl(Resource resource)
        {
            if (resource.Path.StartsWith("http:") || resource.Path.StartsWith("https:"))
            {
                return resource.Path;
            }
            else
            {
                var url = AppRelativeScriptUrl(resource);
                var hash = resource.Hash.ToHexString();
                return url + (url.Contains('?') ? "&" : "?") + hash;
            }
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
            // otherwise the cassette.axd handler is not called.
            // I guess asp.net favours the last file extension it finds.
            var pathWithoutExtension = path.Substring(0, path.Length - ".coffee".Length);
            // Return the URL that will invoke the CoffeeScript compiler to return JavaScript.
            return handler + "/coffee/" + pathWithoutExtension;
        }

        /// <summary>
        /// Returns the application relative URL for each required script module.
        /// The hash of each module is appended to enable long-lived caching that
        /// is broken when a module changes.
        /// </summary>
        IEnumerable<string> ReleaseScriptUrls(string location)
        {
            return scriptReferenceBuilder
                .GetRequiredModules()
                .Where(m => m.Location == location)
                .Select(m => ReleaseScriptUrl(m));
        }

        string ReleaseScriptUrl(Module m)
        {
            if (m.Path.StartsWith("http:") || m.Path.StartsWith("https:"))
            {
                return m.Path;
            }
            else
            {
                return handler + "/scripts/" + m.Path + "_" + m.Hash.ToHexString();
            }
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
                .Select(m => handler + "/styles/" + m.Path + "_" + m.Hash.ToHexString());
        }

        string AppRelativeStylesheetUrl(Resource stylesheet)
        {
            // TODO: Check for .less and .sass files
            return "~/" + stylesheet.Path;
        }
    }
}
