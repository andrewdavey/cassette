using System.Web;
using System.Web.Routing;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Web
{
    public class CassetteRouteHandler_Tests
    {
        [Fact]
        public void GetHttpHandlerCallsDelegatePassingInTheRequestContextAndReturnsTheCreatedHttpHandler()
        {
            RequestContext actualContext = null;
            var stubHandler = Mock.Of<IHttpHandler>();
            var stubHttp = new Mock<HttpContextBase>();
            var stubContext = new Mock<RequestContext>(stubHttp.Object, new RouteData());

            var container = new TinyIoCContainer();
            container.Register(typeof(IHttpHandler), stubHandler);
            var routeHandler = new CassetteRouteHandler<IHttpHandler>(container);

            var actualHttpHandler = routeHandler.GetHttpHandler(stubContext.Object);

            actualHttpHandler.ShouldBeSameAs(stubHandler);
            actualContext.ShouldBeSameAs(stubContext.Object);
        }
    }
}
