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
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Cassette.Configuration;

namespace Cassette.Web
{
    class CassetteApplication : CassetteApplicationBase
    {
        public CassetteApplication(IEnumerable<Bundle> bundles, CassetteSettings settings, CassetteRouting routing, Func<HttpContextBase> getCurrentHttpContext)
            : base(bundles, settings)
        {
            this.getCurrentHttpContext = getCurrentHttpContext;
            this.routing = routing;
        }

        readonly Func<HttpContextBase> getCurrentHttpContext;
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;
        readonly CassetteRouting routing;

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

        internal void InstallRoutes(RouteCollection routes)
        {
            routing.InstallRoutes(routes);
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