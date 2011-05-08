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

        static PageHelper CreatePageHelper(HtmlHelper html)
        {
            var httpContext = html.ViewContext.HttpContext;
            var useModules = KnapsackHttpModule.Manager.Configuration.ShouldUseModules(httpContext);
            var referenceBuilder = httpContext.Items["Knapsack.ReferenceBuilder"] as ReferenceBuilder;
            if (referenceBuilder == null)
            {
                throw new InvalidOperationException("Knapsack.ReferenceBuilder has not been added to the current HttpContext Items. Make sure the KnapsackHttpModule has been added to Web.config.");
            }

            return new PageHelper(useModules, referenceBuilder);
        }
    }
}
