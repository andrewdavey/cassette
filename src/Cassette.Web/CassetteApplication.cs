using System;
using System.Web;
using Cassette.Configuration;

namespace Cassette.Web
{
    class CassetteApplication : CassetteApplicationBase
    {
        public CassetteApplication(IBundleContainer bundleContainer, CassetteSettings settings, Func<HttpContextBase> getCurrentHttpContext)
            : base(bundleContainer, settings)
        {
            this.getCurrentHttpContext = getCurrentHttpContext;
        }

        readonly Func<HttpContextBase> getCurrentHttpContext;
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public void OnPostMapRequestHandler()
        {
            getCurrentHttpContext().Items[PlaceholderTrackerKey] = CreatePlaceholderTracker();
        }

        public void OnPostRequestHandlerExecute()
        {
            var httpContext = getCurrentHttpContext();
            if (!Settings.IsHtmlRewritingEnabled) return;
            if (!httpContext.IsHtmlRewritingEnabled()) return;
            if (!CanRewriteOutput(httpContext)) return;

            var response = httpContext.Response;
            var tracker = (IPlaceholderTracker)httpContext.Items[PlaceholderTrackerKey];
            var filter = new PlaceholderReplacingResponseFilter(
                response,
                tracker
            );
            response.Filter = filter;
        }

        bool CanRewriteOutput(HttpContextBase httpContext)
        {
            var statusCode = httpContext.Response.StatusCode;
            if (300 <= statusCode && statusCode < 400) return false;
            if (statusCode == 401) return false; // 401 gets converted into a redirect by FormsAuthenticationBundle.

            return IsHtmlResponse(httpContext);
        }

        bool IsHtmlResponse(HttpContextBase httpContext)
        {
            return httpContext.Response.ContentType == "text/html" ||
                   httpContext.Response.ContentType == "application/xhtml+xml";
        }

        protected override IPlaceholderTracker GetPlaceholderTracker()
        {
            var items = getCurrentHttpContext().Items;
            return (IPlaceholderTracker)items[PlaceholderTrackerKey];
        }

        protected override IReferenceBuilder GetOrCreateReferenceBuilder(Func<IReferenceBuilder> create)
        {
            var items = getCurrentHttpContext().Items;
            const string key = "Cassette.ReferenceBuilder";
            if (items.Contains(key))
            {
                return (IReferenceBuilder)items[key];
            }
            else
            {
                var builder = create();
                items[key] = builder;
                return builder;
            }
        }
    }
}