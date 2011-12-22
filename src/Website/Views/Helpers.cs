using System;
using System.Web.Mvc;

namespace Website.Views
{
    public static class Helpers
    {
        public static string DocumentationUrl(this UrlHelper urlHelper, string path)
        {
            return urlHelper.RouteUrl("Documentation", new { path });
        }

        public static string ToPublicUrl(this UrlHelper urlHelper, string relativeUrl)
        {
            var httpContext = urlHelper.RequestContext.HttpContext;

            var uriBuilder = new UriBuilder
            {
                Host = httpContext.Request.Url.Host,
                Path = "/",
                Port = 80,
                Scheme = "http",
            };

            if (httpContext.Request.IsLocal)
            {
                uriBuilder.Port = httpContext.Request.Url.Port;
            }

            return new Uri(uriBuilder.Uri, relativeUrl).AbsoluteUri;
        }
    }
}
