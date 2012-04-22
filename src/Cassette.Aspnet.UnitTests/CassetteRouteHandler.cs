using System.Web;
using System.Web.Routing;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Aspnet
{
    public class CassetteRouteHandler_Tests
    {
        [Fact]
        public void GetHttpHandlerReturnHttpHandlerFromContainer()
        {
            var stubHandler = Mock.Of<IHttpHandler>();
            var stubHttpContext = Mock.Of<HttpContextBase>();
            var stubContext = new Mock<RequestContext>(stubHttpContext, new RouteData());

            var container = new TinyIoCContainer();
            container.Register(typeof(IHttpHandler), stubHandler);
            var routeHandler = new CassetteRouteHandler<IHttpHandler>(container);

            var actualHttpHandler = routeHandler.GetHttpHandler(stubContext.Object);

            actualHttpHandler.ShouldBeSameAs(stubHandler);
        }
    }
}