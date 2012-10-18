using System.Web;

namespace Cassette.RequireJS
{
    public static class ViewHelper
    {
        public static IHtmlString RequireJsScript(params string[] initialModules)
        {
            return Instance.RequireJsScript(initialModules);
        }

        public static ViewHelperImplementation Instance { get; set; }
    }
}
