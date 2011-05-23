using System;
using System.Web;
using System.Web.Mvc;

namespace Knapsack.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Records that the calling view requires the given script. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPath">The application relative path to the script file.</param>
        public static void ReferenceScript(this HtmlHelper html, string scriptPath)
        {
            CreatePageHelper(html).ReferenceScript(scriptPath);
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

        // Allow unit tests to change this implementation.
        internal static Func<HtmlHelper, IPageHelper> CreatePageHelper = CreatePageHelperImpl;

        internal static IPageHelper CreatePageHelperImpl(HtmlHelper html)
        {
            var httpContext = html.ViewContext.HttpContext;
            return KnapsackHttpModule.GetPageHelper(httpContext);
        }
    }
}
