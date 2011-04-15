using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace Knapsack.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        public static void AddScriptReference(this HtmlHelper html, string scriptPath)
        {
            var builder = GetReferenceBuilder(html);
            builder.AddReference(scriptPath);
        }

        private static ReferenceBuilder GetReferenceBuilder(HtmlHelper html)
        {
            return (ReferenceBuilder)html.ViewContext.HttpContext.Items["Knapsack.ReferenceBuilder"];
        }

        public static IHtmlString RequiredScripts(this HtmlHelper html)
        {
            var template = "<script src=\"{0}\" type=\"text/javascript\"></script>";

            var builder = GetReferenceBuilder(html);
            var scripts = builder.GetRequiredModules()
                .Select(m => "~/knapsack.axd/" + m.Path)
                .Select(VirtualPathUtility.ToAbsolute)
                .Select(src => string.Format(template, src));

            return new HtmlString(string.Join("\r\n", scripts));
        }
    }
}
