using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Cassette.Utilities;

namespace Cassette.Assets.Stylesheets
{
    public class StylesheetAssetManager
    {
        public StylesheetAssetManager(IReferenceBuilder referenceBuilder, IPlaceholderTracker placeholderTracker, string httpHandlerPath, bool useModules, Func<string, string> virtualPathToAbsolute)
        {
            this.referenceBuilder = referenceBuilder;
            this.placeholderTracker = placeholderTracker;
            this.httpHandlerPath = httpHandlerPath;
            this.useModules = useModules;
            this.virtualPathToAbsolute = virtualPathToAbsolute;
        }

        readonly IReferenceBuilder referenceBuilder;
        readonly IPlaceholderTracker placeholderTracker;
        readonly string httpHandlerPath;
        readonly bool useModules;
        readonly Func<string, string> virtualPathToAbsolute;

        public void Reference(params string[] paths)
        {
            foreach (var path in paths)
            {
                referenceBuilder.AddReference(path);
            }
        }

        public IHtmlString Render(string location = "")
        {
            if (placeholderTracker != null)
            {
                return placeholderTracker.InsertPlaceholder(PlaceholderId(location), () => CreateStylesheetsHtml(location));
            }
            else
            {
                return CreateStylesheetsHtml(location);
            }
        }

        string PlaceholderId(string location)
        {
            return "$$Cassette-Stylesheets-" + location + "$$";
        }

        IHtmlString CreateStylesheetsHtml(string location)
        {
            var cssUrls = useModules
                ? ReleaseStylesheetUrls(location)
                : DebugStylesheetUrls(location);

            var template = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
            var linkElements = BuildHtmlElements(cssUrls, template);

            var allHtml = string.Join("\r\n", linkElements);
            return new HtmlString(allHtml);
        }

        IEnumerable<string> DebugStylesheetUrls(string location)
        {
            return referenceBuilder
                .GetRequiredModules()
                .Where(m => m.Location == location)
                .SelectMany(m => m.Assets)
                .Select(r => new { url = AppRelativeStylesheetUrl(r), hash = r.Hash.ToHexString() })
                .Select(r => r.url + (r.url.Contains('?') ? "&" : "?") + r.hash);
        }

        /// <summary>
        /// Returns the application relative URL for each required stylesheet module.
        /// The hash of each module is appended to enable long-lived caching that
        /// is broken when a module changes.
        /// </summary>
        IEnumerable<string> ReleaseStylesheetUrls(string location)
        {
            return referenceBuilder
                .GetRequiredModules()
                .Where(m => m.Location == location)
                .Select(m => httpHandlerPath + "/styles/" + m.Path + "_" + m.Hash.ToHexString());
        }

        string AppRelativeStylesheetUrl(Asset stylesheet)
        {
            // TODO: Check for .less and .sass files
            return "~/" + stylesheet.Path;
        }

        IEnumerable<string> BuildHtmlElements(IEnumerable<string> urls, string template)
        {
            return urls
                .Select(url => url.StartsWith("http:") || url.StartsWith("https:") ? url : virtualPathToAbsolute(url))
                .Select(HttpUtility.HtmlAttributeEncode)
                .Select(src => string.Format(template, src));
        }
    }
}
