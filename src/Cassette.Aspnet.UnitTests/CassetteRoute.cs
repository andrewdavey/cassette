using System.Web.Routing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class CassetteRoute_Tests
    {
        [Fact]
        public void RouteHandlerIsSetFromConstructorArgument()
        {
            var routeHandler = Mock.Of<IRouteHandler>();
            var route = new CassetteRoute("url", routeHandler);

            route.RouteHandler.ShouldBeSameAs(routeHandler);
        }

        [Fact]
        public void CassetteRouteHasIncomingOnlyConstraint()
        {
            var routeHandler = Mock.Of<IRouteHandler>();
            var route = new CassetteRoute("url", routeHandler);

            var constraint = (IRouteConstraint)route.Constraints["IncomingOnlyConstraint"];
            var isMatch = constraint.Match(null, route, null, null, RouteDirection.IncomingRequest);
            isMatch.ShouldBeTrue();
        }
    }
}
