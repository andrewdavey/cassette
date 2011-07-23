using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.Utilities;

namespace Cassette.Assets.Scripts
{
    public class ScriptAssetManager
    {
        public ScriptAssetManager(IReferenceBuilder referenceBuilder, bool useModules, IPlaceholderTracker placeholderTracker, string httpHandlerPath, Func<string, string> virtualPathToAbsolute)
        {
            this.referenceBuilder = referenceBuilder;
            this.useModules = useModules;
            this.placeholderTracker = placeholderTracker;
            this.httpHandlerPath = httpHandlerPath;
            this.virtualPathToAbsolute = virtualPathToAbsolute;
        }

        readonly IReferenceBuilder referenceBuilder;
        readonly bool useModules;
        readonly IPlaceholderTracker placeholderTracker;
        readonly string httpHandlerPath;
        readonly Func<string, string> virtualPathToAbsolute;

        public void Reference(params string[] paths)
        {
            foreach (var path in paths)
            {
                referenceBuilder.AddReference(path);
            }
        }

        public void ExternalReference(string url, string location = "")
        {
            referenceBuilder.AddExternalReference(url, location);
        }

        public IHtmlString Render(string location = "")
        {
            if (placeholderTracker != null)
            {
                return placeholderTracker.InsertPlaceholder(
                    PlaceholderId(location),
                    () => CreateScriptsHtml(location)
                );
            }
            else
            {
                return CreateScriptsHtml(location);
            }
        }

        string PlaceholderId(string location)
        {
            return "$$Cassette-Scripts-" + location + "$$";
        }

        IHtmlString CreateScriptsHtml(string location)
        {
            var scriptUrls = useModules
                ? ReleaseScriptUrls(location)
                : DebugScriptUrls(location);

            var template = "<script src=\"{0}\" type=\"text/javascript\"></script>";
            var scriptElements = BuildHtmlElements(scriptUrls, template);

            var allHtml = string.Join("\r\n", scriptElements);
            return new HtmlString(allHtml);
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
            return referenceBuilder
                .GetRequiredModules()
                .Where(m => m.Location == location)
                .SelectMany(m => m.Assets)
                .Select(DebugScriptUrl);
        }

        string DebugScriptUrl(Asset asset)
        {
            if (asset.Path.StartsWith("http:") || asset.Path.StartsWith("https:"))
            {
                return asset.Path;
            }
            else
            {
                var url = AppRelativeScriptUrl(asset);
                var hash = asset.Hash.ToHexString();
                return url + (url.Contains('?') ? "&" : "?") + hash;
            }
        }

        string AppRelativeScriptUrl(Asset script)
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
            return httpHandlerPath + "/coffee/" + pathWithoutExtension;
        }

        /// <summary>
        /// Returns the application relative URL for each required script module.
        /// The hash of each module is appended to enable long-lived caching that
        /// is broken when a module changes.
        /// </summary>
        IEnumerable<string> ReleaseScriptUrls(string location)
        {
            return referenceBuilder
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
                return httpHandlerPath + "/scripts/" + m.Path + "_" + m.Hash.ToHexString();
            }
        }
    }
}
