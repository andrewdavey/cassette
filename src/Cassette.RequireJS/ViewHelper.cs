using System;
using System.Linq;
using System.Web;

// ReSharper disable CheckNamespace
namespace Cassette.Views
// ReSharper restore CheckNamespace
{
    public static class RequireJs
    {
        public static IHtmlString RenderScripts(params string[] initialModulePaths)
        {
            // Reference the require.js configuration script asset.
            // This was inserted into the main bundle, which includes require.js itself.
            Bundles.Reference("~/Cassette.RequireJs");

            return new HtmlString(
                Bundles.RenderScripts().ToHtmlString() +
                CreateRequireCallJavaScript(initialModulePaths)
            );
        }

        static string CreateRequireCallJavaScript(string[] initialModulePaths)
        {
            var jsonSerializer = new SimpleJsonSerializer();
            var modulePathsArray = jsonSerializer.Serialize(initialModulePaths);
            return "<script>require(" + modulePathsArray + ")</script>";
        }

        public static IHtmlString RenderCallScript(params string[] initialModulePaths)
        {
            return new HtmlString(CreateRequireCallJavaScript(initialModulePaths));
        }
    }
}