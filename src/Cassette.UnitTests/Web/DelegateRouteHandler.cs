using System.Web;
using System.Web.Routing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class DelegateRouteHandler_Tests
    {
        [Fact]
        public void GetHttpHandlerCallsDelegatePassingInTheRequestContextAndReturnsTheCreatedHttpHandler()
        {
            RequestContext actualContext = null;
            var stubHandler = Mock.Of<IHttpHandler>();
            var stubHttp = new Mock<HttpContextBase>();
            var stubContext = new Mock<RequestContext>(stubHttp.Object, new RouteData());

            var routeHandler = new DelegateRouteHandler(context => {
                actualContext = context;
                return stubHandler;
            });

            var actualHttpHandler = routeHandler.GetHttpHandler(stubContext.Object);

            actualHttpHandler.ShouldBeSameAs(stubHandler);
            actualContext.ShouldBeSameAs(stubContext.Object);
        }
    }
}
