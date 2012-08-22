using System;
using System.Collections.Generic;
using System.Web;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Views
{
    /// <summary>
    /// Cassette API facade used by views to reference bundles and render the required HTML elements.
    /// </summary>
    /// <remarks>
    /// Keep overloads to stay compatible with .NET 3.5 (otherwise consumers will get exceptions)
    /// </remarks>
    public static class Bundles
    {
        /// <summary>
        /// The helper implementation used by the static methods of this class.
        /// </summary>
        public static IBundlesHelper Helper { get; set; }
        
        /// <summary>
        /// Adds a reference to a bundle for the current page.
        /// </summary>
        /// <param name="assetPathOrBundlePathOrUrl">Either an application relative path to an asset file or bundle. Or a URL of an external resource.</param>
        public static void Reference(string assetPathOrBundlePathOrUrl)
        {
            Reference(assetPathOrBundlePathOrUrl, null);
        }

        /// <summary>
        /// Adds a reference to a bundle for the current page.
        /// </summary>
        /// <param name="assetPathOrBundlePathOrUrl">Either an application relative path to an asset file or bundle. Or a URL of an external resource.</param>
        public static void Reference<T>(string assetPathOrBundlePathOrUrl)
            where T : Bundle
        {
            Reference<T>(assetPathOrBundlePathOrUrl, null);
        }

        /// <summary>
        /// Adds a reference to a bundle for the current page.
        /// </summary>
        /// <param name="assetPathOrBundlePathOrUrl">Either an application relative path to an asset file or bundle. Or a URL of an external resource.</param>
        /// <param name="pageLocation">The optional page location of the referenced bundle. This controls where it will be rendered.</param>
        public static void Reference(string assetPathOrBundlePathOrUrl, string pageLocation)
        {
            Helper.Reference(assetPathOrBundlePathOrUrl, pageLocation);
        }

        /// <summary>
        /// Adds a reference to a bundle for the current page.
        /// </summary>
        /// <param name="assetPathOrBundlePathOrUrl">Either an application relative path to an asset file or bundle. Or a URL of an external resource.</param>
        /// <param name="pageLocation">The optional page location of the referenced bundle. This controls where it will be rendered.</param>
        public static void Reference<T>(string assetPathOrBundlePathOrUrl, string pageLocation)
            where T : Bundle
        {
            Helper.Reference<T>(assetPathOrBundlePathOrUrl, pageLocation);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The JavaScript code.</param>
        public static void AddInlineScript(string scriptContent)
        {
            AddInlineScript(scriptContent, null, null);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The JavaScript code.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        public static void AddInlineScript(string scriptContent, string pageLocation) {
            AddInlineScript(scriptContent, pageLocation, null);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The JavaScript code.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        /// <param name="customizeBundle">The optional delegate used to customize the created bundle before adding it to the collection.</param>
        public static void AddInlineScript(string scriptContent, string pageLocation, Action<ScriptBundle> customizeBundle)
        {
            var bundle = new InlineScriptBundle(scriptContent);

            AddScriptBundle(bundle, pageLocation, customizeBundle);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The Razor template for the Javascript code.</param>        
        /// <code lang="CS">
        /// @{
        ///   Bundles.AddInlineScript(@&lt;text&gt;
        ///     var foo = "Hello World";
        ///     alert( foo );&lt;/text&gt;);
        /// }
        /// </code>
        public static void AddInlineScript(Func<object, object> scriptContent)
        {
            AddInlineScript(scriptContent, null, null);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The Razor template for the Javascript code.</param>     
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>   
        /// <code lang="CS">
        /// @{
        ///   Bundles.AddInlineScript(@&lt;text&gt;
        ///     var foo = "Hello World";
        ///     alert( foo );&lt;/text&gt;);
        /// }
        /// </code>
        public static void AddInlineScript(Func<object, object> scriptContent, string pageLocation) {
            AddInlineScript(scriptContent, pageLocation, null);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The Razor template for the Javascript code.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        /// <param name="customizeBundle">The optional delegate used to customize the created bundle before adding it to the collection.</param>
        /// <code lang="CS">
        /// @{
        ///   Bundles.AddInlineScript(@&lt;text&gt;
        ///     var foo = "Hello World";
        ///     alert( foo );&lt;/text&gt;);
        /// }
        /// </code>
        public static void AddInlineScript(Func<object, object> scriptContent, string pageLocation, Action<ScriptBundle> customizeBundle)
        {
            AddInlineScript(scriptContent(null).ToString(), pageLocation, customizeBundle);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The data object, serialized into JSON.</param>        
        public static void AddPageData(string globalVariable, object data)
        {
            AddPageData(globalVariable, data, null, null);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The data object, serialized into JSON.</param>        
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        public static void AddPageData(string globalVariable, object data, string pageLocation) {
            AddPageData(globalVariable, data, pageLocation, null);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The data object, serialized into JSON.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        /// <param name="customizeBundle">The optional delegate used to customize the created bundle before adding it to the collection.</param>
        public static void AddPageData(string globalVariable, object data, string pageLocation, Action<ScriptBundle> customizeBundle)
        {
            AddScriptBundle(new PageDataScriptBundle(globalVariable, data, Helper.JsonSerializer), pageLocation, customizeBundle);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The dictionary of data, serialized into JSON.</param>        
        public static void AddPageData(string globalVariable, IEnumerable<KeyValuePair<string, object>> data)
        {
            AddPageData(globalVariable, data, null, null);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The dictionary of data, serialized into JSON.</param>        
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        public static void AddPageData(string globalVariable, IEnumerable<KeyValuePair<string, object>> data, string pageLocation) {
            AddPageData(globalVariable, data, pageLocation, null);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The dictionary of data, serialized into JSON.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        /// <param name="customizeBundle">The optional delegate used to customize the created bundle before adding it to the collection.</param>
        public static void AddPageData(string globalVariable, IEnumerable<KeyValuePair<string, object>> data, string pageLocation, Action<ScriptBundle> customizeBundle)
        {
            AddScriptBundle(new PageDataScriptBundle(globalVariable, data, Helper.JsonSerializer), pageLocation, customizeBundle);
        }

        static void AddScriptBundle(ScriptBundle bundle, string pageLocation, Action<ScriptBundle> customizeBundle)
        {
            if (customizeBundle != null)
            {
                customizeBundle(bundle);
            }

            Helper.Reference(bundle, pageLocation);
        }

        /// <summary>
        /// Renders the required HTML elements for the scripts referenced by the current page.
        /// </summary>        
        /// <returns>HTML script elements.</returns>
        public static IHtmlString RenderScripts()
        {
            return RenderScripts(null);
        }

        /// <summary>
        /// Renders the required HTML elements for the scripts referenced by the current page.
        /// </summary>
        /// <param name="pageLocation">The optional page location being rendered. Only scripts with this location are rendered.</param>
        /// <returns>HTML script elements.</returns>
        public static IHtmlString RenderScripts(string pageLocation)
        {
            return Render<ScriptBundle>(pageLocation);
        }

        /// <summary>
        /// Renders the required HTML elements for the stylesheets referenced by the current page.
        /// </summary>        
        /// <returns>HTML stylesheet link elements.</returns>
        public static IHtmlString RenderStylesheets()
        {
            return RenderStylesheets(null);
        }

        /// <summary>
        /// Renders the required HTML elements for the stylesheets referenced by the current page.
        /// </summary>
        /// <param name="pageLocation">The optional page location being rendered. Only stylesheets with this location are rendered.</param>
        /// <returns>HTML stylesheet link elements.</returns>
        public static IHtmlString RenderStylesheets(string pageLocation)
        {
            return Render<StylesheetBundle>(pageLocation);
        }

        /// <summary>
        /// Renders the required HTML elements for the HTML templates referenced by the current page.
        /// </summary>        
        /// <returns>HTML script elements.</returns>
        public static IHtmlString RenderHtmlTemplates()
        {
            return RenderHtmlTemplates(null);
        }

        /// <summary>
        /// Renders the required HTML elements for the HTML templates referenced by the current page.
        /// </summary>
        /// <param name="pageLocation">The optional page location being rendered. Only HTML templates with this location are rendered.</param>
        /// <returns>HTML script elements.</returns>
        public static IHtmlString RenderHtmlTemplates(string pageLocation)
        {
            return Render<HtmlTemplateBundle>(pageLocation);
        }

        /// <summary>
        /// Returns the URL of the bundle with the given path.
        /// </summary>
        /// <param name="bundlePath">The path of the bundle.</param>
        /// <returns>The URL of the bundle.</returns>
        public static string Url(string bundlePath)
        {
            return Helper.Url<Bundle>(bundlePath);
        }

        /// <summary>
        /// Returns the URL of the bundle with the given path.
        /// </summary>
        /// <param name="bundlePath">The path of the bundle.</param>
        /// <typeparamref name="T">Type of bundle.</typeparamref>
        /// <returns>The URL of the bundle.</returns>
        public static string Url<T>(string bundlePath)
            where T : Bundle
        {
            return Helper.Url<T>(bundlePath);
        }

        /// <summary>
        /// Gets the bundles that have been referenced during the current request.
        /// </summary>        
        public static IEnumerable<Bundle> GetReferencedBundles()
        {
            return GetReferencedBundles(null);
        }

        /// <summary>
        /// Gets the bundles that have been referenced during the current request.
        /// </summary>
        /// <param name="pageLocation">Optional. The page location of bundles to return.</param>
        public static IEnumerable<Bundle> GetReferencedBundles(string pageLocation)
        {
            return Helper.GetReferencedBundles(pageLocation);
        }

        /// <summary>
        /// Get the URLs for bundles that have been referenced during the current request.
        /// </summary>
        /// <typeparam name="T">The type of bundles.</typeparam>        
        public static IEnumerable<string> GetReferencedBundleUrls<T>()
            where T : Bundle
        {
            return GetReferencedBundleUrls<T>(null);
        }

        /// <summary>
        /// Get the URLs for bundles that have been referenced during the current request.
        /// </summary>
        /// <typeparam name="T">The type of bundles.</typeparam>
        /// <param name="pageLocation">Optional. The page location of bundles to return.</param>
        public static IEnumerable<string> GetReferencedBundleUrls<T>(string pageLocation)
            where T : Bundle
        {
            return Helper.GetReferencedBundleUrls<T>(pageLocation);
        }

        static IHtmlString Render<T>(string location) where T : Bundle
        {
            return Helper.Render<T>(location);
        }

        /// <summary>
        /// Returns the Cassette cache-friendly URL for a file, such as an image.
        /// </summary>
        /// <param name="applicationRelativeFilePath">The application relative file path of the file.</param>
        public static string FileUrl(string applicationRelativeFilePath)
        {
            return Helper.FileUrl(applicationRelativeFilePath);
        }

        public static void RebuildCache()
        {
            Helper.RebuildBundleCache();
        }
    }
}