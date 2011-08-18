using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetModule_Render_Tests
    {
        public StylesheetModule_Render_Tests()
        {
            application = new Mock<ICassetteApplication>();
            var urlGenerator = new Mock<IUrlGenerator>();
            application.SetupGet(a => a.UrlGenerator).Returns(urlGenerator.Object);
            module = new StylesheetModule("test");
            asset1 = Mock.Of<IAsset>();
            asset2 = Mock.Of<IAsset>();
            module.Assets.Add(asset1);
            module.Assets.Add(asset2);

            application.Setup(a => a.CreateModuleUrl(module)).Returns("/url");
            urlGenerator.Setup(g => g.CreateAssetUrl(module, asset1)).Returns("/url1");
            urlGenerator.Setup(g => g.CreateAssetUrl(module, asset2)).Returns("/url2");
        }

        readonly Mock<ICassetteApplication> application;
        readonly StylesheetModule module;
        readonly IAsset asset1, asset2;
        
        [Fact]
        public void GivenApplicationIsOutputOptimized_WhenRender_ThenModuleStylesheetLinkReturned()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);

            var html = module.Render(application.Object);

            html.ToHtmlString().ShouldEqual(
                "<link href=\"/url\" type=\"text/css\" rel=\"stylesheet\"/>"
            );
        }

        [Fact]
        public void GivenApplicationIsNotOutputOptimized_WhenRender_ThenAssetScriptsReturned()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(false);

            var html = module.Render(application.Object);

            html.ToHtmlString().ShouldEqual(
                "<link href=\"/url1\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine +
                "<link href=\"/url2\" type=\"text/css\" rel=\"stylesheet\"/>"
            );
        }

        [Fact]
        public void GivenApplicationIsOutputOptimized_WhenMediaSetAndRender_ThenModuleLinksReturnedWithMediaAttribute()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);

            module.Media = "print";
            var html = module.Render(application.Object);

            html.ToHtmlString().ShouldEqual(
                "<link href=\"/url\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>"
            );
        }

        [Fact]
        public void GivenApplicationIsNotOutputOptimized_WhenMediaSetAndRender_ThenAssetLinksReturnedWithMediaAttribute()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(false);

            module.Media = "print";
            var html = module.Render(application.Object);

            html.ToHtmlString().ShouldEqual(
                "<link href=\"/url1\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>" + Environment.NewLine +
                "<link href=\"/url2\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>"
            );
        }
    }
}
