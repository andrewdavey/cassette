using System;
using System.Web;

namespace Cassette.Aspnet
{
    /// <summary>
    /// Rewrites HTML output so Cassette HTML placeholders will be replaced with their actual HTML.
    /// </summary>
    public class PlaceholderRewriter
    {
        readonly CassetteSettings settings;
        readonly Func<IPlaceholderTracker> createPlaceholderTracker;
        readonly Func<HttpContextBase> getHttpContext;

        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public PlaceholderRewriter(CassetteSettings settings, Func<IPlaceholderTracker> createPlaceholderTracker, Func<HttpContextBase> getHttpContext)
        {
            this.settings = settings;
            this.createPlaceholderTracker = createPlaceholderTracker;
            this.getHttpContext = getHttpContext;
        }

        public void AddPlaceholderTrackerToHttpContextItems()
        {
            var tracker = createPlaceholderTracker();
            getHttpContext().Items[PlaceholderTrackerKey] = tracker;
        }

        public void RewriteOutput()
        {
            var httpContext = getHttpContext();
            if (CanRewriteOutput(httpContext))
            {
                InstallPlaceholderReplacingResponseFilter(httpContext);
            }
        }

        void InstallPlaceholderReplacingResponseFilter(HttpContextBase httpContext)
        {
            var response = httpContext.Response;
            var tracker = (IPlaceholderTracker)httpContext.Items[PlaceholderTrackerKey];
            var filter = new PlaceholderReplacingResponseFilter(response.Filter, response.Output.Encoding, response.Headers, tracker);
            response.Filter = filter;
        }

        bool CanRewriteOutput(HttpContextBase httpContext)
        {
            if (!settings.IsHtmlRewritingEnabled) return false;
            if (!httpContext.IsHtmlRewritingEnabled()) return false;

            var statusCode = httpContext.Response.StatusCode;
            if (300 <= statusCode && statusCode < 400) return false;
            if (statusCode == 401) return false; // 401 gets converted into a redirect by FormsAuthenticationModule.

            return IsHtmlResponse(httpContext);
        }

        bool IsHtmlResponse(HttpContextBase httpContext)
        {
            return httpContext.Response.ContentType.StartsWith("text/html") ||
                   httpContext.Response.ContentType.StartsWith("application/xhtml+xml");
        }
    }
}