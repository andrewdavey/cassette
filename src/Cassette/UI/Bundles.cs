#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Web;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.UI
{
    /// <summary>
    /// Cassette API facade used by views to reference bundles and render the required HTML elements.
    /// </summary>
    public static class Bundles
    {
        internal static Func<CassetteApplicationBase> GetApplication;

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
            var bundle = GetApplication().BundleContainer.FindBundleContainingPath(bundlePath);
            if (bundle == null)
            {
                throw new ArgumentException(string.Format("Bundle not found with path \"{0}\".", bundlePath));
            }

            return GetApplication().UrlGenerator.CreateBundleUrl(bundle);
        }

        static IHtmlString Render<T>(string location) where T : Bundle
        {
            return ReferenceBuilder.Render<T>(location);            
        }

        static IReferenceBuilder ReferenceBuilder
        {
            get
            {
                if (GetApplication == null)
                {
                    // We rely on Cassette.Web (or some other) integration library to hook up its application object.
                    // If the delegate is null then the developer probably forgot to reference the integration library.
                    throw new InvalidOperationException("Make sure a Cassette integration library has been referenced. For example, reference Cassette.Web.dll");
                }
                return GetApplication().GetReferenceBuilder();
            }
        }
    }
}