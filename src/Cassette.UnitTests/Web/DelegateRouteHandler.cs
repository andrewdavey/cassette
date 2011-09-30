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
        public void GetHttpHandlerCallsDelegatePassingInTheRequestContext()
        {
            RequestContext actualContext = null;
            var stubHandler = Mock.Of<IHttpHandler>();
            var stubContext = Mock.Of<RequestContext>();

            var routeHandler = new DelegateRouteHandler(context => {
                actualContext = context;
                return stubHandler;
            });

            var actualHttpHandler = routeHandler.GetHttpHandler(stubContext);

            actualHttpHandler.ShouldBeSameAs(stubHandler);
            actualContext.ShouldBeSameAs(stubContext);
        }
    }
}