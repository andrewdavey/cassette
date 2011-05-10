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

        /// <summary>
        /// Creates HTML script elements for all required scripts and their dependencies.
        /// </summary>
        public static IHtmlString RenderScripts(this HtmlHelper html)
        {
           return CreatePageHelper(html).RenderScripts();
        }

        // Allow unit tests to change this implementation.
        internal static Func<HtmlHelper, IPageHelper> CreatePageHelper = CreatePageHelperImpl;

        internal static IPageHelper CreatePageHelperImpl(HtmlHelper html)
        {
            var httpContext = html.ViewContext.HttpContext;
            var helper = httpContext.Items["Knapsack.PageHelper"] as IPageHelper;
            if (helper == null)
            {
                throw new InvalidOperationException("Knapsack.PageHelper has not been added to the current HttpContext Items. Make sure the KnapsackHttpModule has been added to Web.config.");
            }

            return helper;
        }
    }
}
