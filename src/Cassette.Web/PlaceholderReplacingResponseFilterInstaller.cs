using System;
using System.Web;
using Cassette.Configuration;

namespace Cassette.Web
{
    /// <summary>
    /// Hooks into the ASP.NET HTTP request pipeline to rewrite HTML output so Cassette HTML placeholders will be replaced with their actual HTML.
    /// </summary>
    class PlaceholderReplacingResponseFilterInstaller
    {
        readonly CassetteSettings settings;
        readonly Func<IPlaceholderTracker> createPlaceholderTracker;
        readonly Func<HttpContextBase> getHttpContext;

        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public PlaceholderReplacingResponseFilterInstaller(CassetteSettings settings, Func<IPlaceholderTracker> createPlaceholderTracker, Func<HttpContextBase> getHttpContext)
        {
            this.settings = settings;
            this.createPlaceholderTracker = createPlaceholderTracker;
            this.getHttpContext = getHttpContext;
        }

        public void Install(HttpApplication httpApplication)
        {
            httpApplication.PostMapRequestHandler += ApplicationOnPostMapRequestHandler;
            httpApplication.PostRequestHandlerExecute += ApplicationOnPostRequestHandlerExecute;
        }

        void ApplicationOnPostMapRequestHandler(object sender, EventArgs eventArgs)
        {
            AddPlaceholderTrackerToHttpContextItems();
        }

        void AddPlaceholderTrackerToHttpContextItems()
        {
            var tracker = createPlaceholderTracker();
            getHttpContext().Items[PlaceholderTrackerKey] = tracker;
        }

        void ApplicationOnPostRequestHandlerExecute(object sender, EventArgs eventArgs)
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
            var filter = new PlaceholderReplacingResponseFilter(response, tracker);
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
            return httpContext.Response.ContentType == "text/html" ||
                   httpContext.Response.ContentType == "application/xhtml+xml";
        }
    }
}