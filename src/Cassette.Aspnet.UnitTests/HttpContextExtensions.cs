using System.Collections.Generic;
using System.Web;
using System.Web.Handlers;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class HttpContextExtensions_Tests
    {
        readonly HttpContextBase context;
        readonly Dictionary<string, object> items;
        IHttpHandler currentHandler;

        public HttpContextExtensions_Tests()
        {
            var contextMock = new Mock<HttpContextBase>();
            items = new Dictionary<string, object>();
            contextMock.Setup(c => c.Items).Returns(items);
            contextMock.Setup(c => c.CurrentHandler).Returns(() => currentHandler);
            context = contextMock.Object;
        }

        [Fact]
        public void WhenDisableHtmlRewriting_ThenCassetteIsHtmlRewritingEnabledAddedToContextItems()
        {
            context.DisableHtmlRewriting();
            items.ContainsKey("Cassette.IsHtmlRewritingEnabled").ShouldBeTrue();
        }

        [Fact]
        public void IsHtmlRewritingEnabledIsTrueByDefault()
        {
            context.IsHtmlRewritingEnabled().ShouldBeTrue();
        }

        [Fact]
        public void WhenDisableHtmlRewriting_ThenIsHtmlRewritingEnabledIsFalse()
        {
            context.DisableHtmlRewriting();
            context.IsHtmlRewritingEnabled().ShouldBeFalse();
        }

        [Fact]
        public void WhenHandlerIsAssemblyResourceLoader_ThenIsHtmlRewritingEnabledIsFalse()
        {
            currentHandler = new AssemblyResourceLoader();
            context.IsHtmlRewritingEnabled().ShouldBeFalse();
        }
    }
}