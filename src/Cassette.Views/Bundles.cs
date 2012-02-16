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
    public static class Bundles
    {
        /// <summary>
        /// Adds a reference to a bundle for the current page.
        /// </summary>
        /// <param name="assetPathOrBundlePathOrUrl">Either an application relative path to an asset file or bundle. Or a URL of an external resource.</param>
        /// <param name="pageLocation">The optional page location of the referenced bundle. This controls where it will be rendered.</param>
        public static void Reference(string assetPathOrBundlePathOrUrl, string pageLocation = null)
        {
            ReferenceBuilder.Reference(assetPathOrBundlePathOrUrl, pageLocation);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The JavaScript code.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        public static void AddInlineScript(string scriptContent, string pageLocation = null)
        {
            ReferenceBuilder.Reference(new InlineScriptBundle(scriptContent), pageLocation);
        }

        /// <summary>
        /// Adds a page reference to an inline JavaScript block.
        /// </summary>
        /// <param name="scriptContent">The Razor template for the Javascript code.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        /// <code lang="CS">
        /// @{
        ///   Bundles.AddInlineScript(@<text>
        ///     var foo = "Hello World";
        ///     alert( foo );</text>);
        /// }
        /// </code>
        public static void AddInlineScript(Func<object, object> scriptContent, string pageLocation = null)
        {
            AddInlineScript(scriptContent(null).ToString(), pageLocation);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The data object, serialized into JSON.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        public static void AddPageData(string globalVariable, object data, string pageLocation = null)
        {
            ReferenceBuilder.Reference(new PageDataScriptBundle(globalVariable, data), pageLocation);
        }

        /// <summary>
        /// Add a page reference to a script that initializes a global JavaScript variable with the given data.
        /// </summary>
        /// <param name="globalVariable">The name of the global JavaScript variable to assign.</param>
        /// <param name="data">The dictionary of data, serialized into JSON.</param>
        /// <param name="pageLocation">The optional page location of the script. This controls where it will be rendered.</param>
        public static void AddPageData(string globalVariable, IEnumerable<KeyValuePair<string, object>> data, string pageLocation = null)
        {
            ReferenceBuilder.Reference(new PageDataScriptBundle(globalVariable, data), pageLocation);
        }

        /// <summary>
        /// Renders the required HTML elements for the scripts referenced by the current page.
        /// </summary>
        /// <param name="pageLocation">The optional page location being rendered. Only scripts with this location are rendered.</param>
        /// <returns>HTML script elements.</returns>
        public static IHtmlString RenderScripts(string pageLocation = null)
        {
            return Render<ScriptBundle>(pageLocation);
        }

        /// <summary>
        /// Renders the required HTML elements for the stylesheets referenced by the current page.
        /// </summary>
        /// <param name="pageLocation">The optional page location being rendered. Only stylesheets with this location are rendered.</param>
        /// <returns>HTML stylesheet link elements.</returns>
        public static IHtmlString RenderStylesheets(string pageLocation = null)
        {
            return Render<StylesheetBundle>(pageLocation);
        }

        /// <summary>
        /// Renders the required HTML elements for the HTML templates referenced by the current page.
        /// </summary>
        /// <param name="pageLocation">The optional page location being rendered. Only HTML templates with this location are rendered.</param>
        /// <returns>HTML script elements.</returns>
        public static IHtmlString RenderHtmlTemplates(string pageLocation = null)
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
            var bundle = Application.FindBundleContainingPath<Bundle>(bundlePath);
            if (bundle == null)
            {
                throw new ArgumentException(string.Format("Bundle not found with path \"{0}\".", bundlePath));
            }

            return Application.Settings.UrlGenerator.CreateBundleUrl(bundle);
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
            var bundle = Application.FindBundleContainingPath<T>(bundlePath);
            if (bundle == null)
            {
                throw new ArgumentException(string.Format("Bundle not found with path \"{0}\".", bundlePath));
            }

            return Application.Settings.UrlGenerator.CreateBundleUrl(bundle);
        }

        static IHtmlString Render<T>(string location) where T : Bundle
        {
            return new HtmlString(ReferenceBuilder.Render<T>(location));
        }

        static IReferenceBuilder ReferenceBuilder
        {
            get
            {
                return Application.GetReferenceBuilder();
            }
        }

        static ICassetteApplication Application
        {
            get { return CassetteApplicationContainer.Application; }
        }
    }
}