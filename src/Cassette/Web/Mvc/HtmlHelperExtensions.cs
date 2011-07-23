using System;
using System.Web;
using System.Web.Mvc;

namespace Cassette.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Records that the calling view requires the given script. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPathOrUrl">The application relative path to the script file or an absolute external script URL.</param>
        public static void ReferenceScript(this HtmlHelper html, string scriptPathOrUrl)
        {
            CreatePageHelper(html).ReferenceScript(scriptPathOrUrl);
        }
        
        /// <summary>
        /// Records that the calling view requires the given script. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPath">The absolute external script URL.</param>
        /// <param name="location">The location identifier for this script e.g. "head" or "body".</param>
        public static void ReferenceScript(this HtmlHelper html, string externalScriptUrl, string location)
        {
            CreatePageHelper(html).ReferenceExternalScript(externalScriptUrl, location);
        }

        /// <summary>
        /// Records that the calling view requires the given stylesheet. This does not render any
        /// HTML. Call <see cref="RenderStylesheetLinks"/> to actually output the link elements.
        /// </summary>
        /// <param name="stylesheetPath">The application relative path to the stylesheet file.</param>
        public static void ReferenceStylesheet(this HtmlHelper html, string stylesheetPath)
        {
            CreatePageHelper(html).ReferenceStylesheet(stylesheetPath);
        }

        public static void ReferenceHtmlTemplate(this HtmlHelper html, string htmlTemplatePath)
        {
            CreatePageHelper(html).ReferenceHtmlTemplate(htmlTemplatePath);
        }

        /// <summary>
        /// Creates HTML script elements for all required scripts and their dependencies.
        /// </summary>
        public static IHtmlString RenderScripts(this HtmlHelper html, string location = null)
        {
           return CreatePageHelper(html).RenderScripts(location);
        }

        /// <summary>
        /// Creates HTML link elements for all required stylesheets and their dependencies.
        /// </summary>
        public static IHtmlString RenderStylesheetLinks(this HtmlHelper html)
        {
            return CreatePageHelper(html).RenderStylesheetLinks();
        }

        public static IHtmlString RenderHtmlTemplates(this HtmlHelper html)
        {
            return CreatePageHelper(html).RenderHtmlTemplates();
        }

        // Allow unit tests to change this implementation.
        internal static Func<HtmlHelper, IPageHelper> CreatePageHelper = CreatePageHelperImpl;

        internal static IPageHelper CreatePageHelperImpl(HtmlHelper html)
        {
            var httpContext = html.ViewContext.HttpContext;
            return CassetteHttpModule.GetPageHelper(httpContext);
        }
    }
}
