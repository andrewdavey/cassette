#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Web;
using System.Web.Handlers;
using Cassette.Configuration;
using Cassette.UI;
using System.Web.Routing;

namespace Cassette.Web
{
    class CassetteApplication : CassetteApplicationBase
    {
        public CassetteApplication(BundleCollection bundles, CassetteSettings settings, CassetteRouting routing, RouteCollection routes, Func<HttpContextBase> getCurrentHttpContext)
            : base(bundles, settings, routing)
        {
            this.getCurrentHttpContext = getCurrentHttpContext;

            routing.InstallRoutes(routes, BundleContainer);
        }

        readonly Func<HttpContextBase> getCurrentHttpContext;
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public void OnPostMapRequestHandler(HttpContextBase httpContext)
        {
            IPlaceholderTracker tracker;
            if (HtmlRewritingEnabled)
            {
                tracker = new PlaceholderTracker();
            }
            else
            {
                tracker = new NullPlaceholderTracker();
            }
            httpContext.Items[PlaceholderTrackerKey] = tracker;
        }

        public void OnPostRequestHandlerExecute(HttpContextBase httpContext)
        {
            if (!HtmlRewritingEnabled) return;
            
            if (httpContext.CurrentHandler is AssemblyResourceLoader)
            {
                // The AssemblyResourceLoader handler (for WebResource.axd requests) prevents further writes via some internal puke code.
                // This prevents response filters from working. The result is an empty response body!
                // So don't bother installing a filter for these requests. We don't need to rewrite them anyway.
                return;
            }

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

        protected override IReferenceBuilder<T> GetOrCreateReferenceBuilder<T>(Func<IReferenceBuilder<T>> create)
        {
            var items = getCurrentHttpContext().Items;
            var key = "ReferenceBuilder:" + typeof(T).FullName;
            if (items.Contains(key))
            {
                return (IReferenceBuilder<T>)items[key];
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

