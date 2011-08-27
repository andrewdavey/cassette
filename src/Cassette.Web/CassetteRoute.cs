using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    class CassetteRoute : Route
    {
        public CassetteRoute(string url, IRouteHandler routeHandler) 
            : base(
                url,
                new RouteValueDictionary(),
                new RouteValueDictionary
                {
                    {"IncomingOnlyConstraint", new OnlyMatchIncomingRequestsConstraint()}
                },
                routeHandler
            )
        {
        }

        class OnlyMatchIncomingRequestsConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                return routeDirection == RouteDirection.IncomingRequest;
            }
        }
    }
}
