using System.Web;
using System.Web.Handlers;

namespace Cassette.Web
{
    static class HttpContextExtensions
    {
        const string IsHtmlRewritingEnabledKey = "Cassette.IsHtmlRewritingEnabled";

        public static void DisableHtmlRewriting(this HttpContextBase context)
        {
            context.Items.Add(IsHtmlRewritingEnabledKey, false);
        }

        public static bool IsHtmlRewritingEnabled(this HttpContextBase httpContext)
        {
            if (httpContext.Items.Contains(IsHtmlRewritingEnabledKey) 
                && (bool)httpContext.Items[IsHtmlRewritingEnabledKey] == false)
            {
                return false;
            }

            if (httpContext.CurrentHandler is AssemblyResourceLoader)
            {
                // The AssemblyResourceLoader handler (for WebResource.axd requests) prevents further writes via some internal puke code.
                // This prevents response filters from working. The result is an empty response body!
                // So don't bother installing a filter for these requests. We don't need to rewrite them anyway.
                return false;
            }

            return true;
        }
    }
}