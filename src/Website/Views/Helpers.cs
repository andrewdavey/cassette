using System.Web.Mvc;

namespace Website.Views
{
    public static class Helpers
    {
        public static string DocumentationUrl(this UrlHelper urlHelper, string path)
        {
            return urlHelper.RouteUrl("Documentation", new { path });
        }
    }
}