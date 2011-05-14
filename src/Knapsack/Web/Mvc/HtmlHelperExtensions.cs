using System;
using System.Web;
using System.Web.Mvc;

namespace Knapsack.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Records that the calling view requires the given script path. This does not render any
        /// HTML. Call <see cref="RenderScripts"/> to actually output the script elements.
        /// </summary>
        /// <param name="scriptPath">The application relative path to the script file.</param>
        public static void AddScriptReference(this HtmlHelper html, string scriptPath)
        {
            CreatePageHelper(html).AddScriptReference(scriptPath);
        }

        public static void AddStylesheet(this HtmlHelper html, string cssPath)
        {
            CreatePageHelper(html).AddStylesheet(cssPath);
        }

        /// <summary>
        /// Creates HTML script elements for all required scripts and their dependencies.
        /// </summary>
        public static IHtmlString RenderScripts(this HtmlHelper html)
        {
           return CreatePageHelper(html).RenderScripts();
        }

        public static IHtmlString RenderStyleLinks(this HtmlHelper html)
        {
            return CreatePageHelper(html).RenderStyleLinks();
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
