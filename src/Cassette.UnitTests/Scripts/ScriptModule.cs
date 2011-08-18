using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptModule_Render_Tests
    {
        public ScriptModule_Render_Tests()
        {
            application = new Mock<ICassetteApplication>();
            var urlGenerator = new Mock<IUrlGenerator>();
            application.SetupGet(a => a.UrlGenerator).Returns(urlGenerator.Object);
            module = new ScriptModule("test");
            asset1 = Mock.Of<IAsset>();
            asset2 = Mock.Of<IAsset>();
            module.Assets.Add(asset1);
            module.Assets.Add(asset2);

            application.Setup(a => a.CreateModuleUrl(module)).Returns("/url");
            urlGenerator.Setup(g => g.CreateAssetUrl(module, asset1)).Returns("/url1");
            urlGenerator.Setup(g => g.CreateAssetUrl(module, asset2)).Returns("/url2");
        }

        readonly Mock<ICassetteApplication> application;
        readonly ScriptModule module;
        readonly IAsset asset1, asset2;

        [Fact]
        public void GivenApplicationIsOutputOptimized_WhenRender_ThenModuleScriptReturned()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);

            var html = module.Render(application.Object);

            html.ToHtmlString().ShouldEqual("<script src=\"/url\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void GivenApplicationIsNotOutputOptimized_WhenRender_ThenAssetScriptsReturned()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(false);

            var html = module.Render(application.Object);

            html.ToHtmlString().ShouldEqual(
                "<script src=\"/url1\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<script src=\"/url2\" type=\"text/javascript\"></script>"
            );
        }
    }
}
