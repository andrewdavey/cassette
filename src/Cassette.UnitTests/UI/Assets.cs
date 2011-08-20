using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class Assets_Test
    {
        [Fact]
        public void ScriptsReturnsPageAssetManager()
        {
            var application = new Mock<ICassetteApplication>();
            Assets.Application = application.Object;
            var manager = new Mock<IPageAssetManager>();
            application.Setup(a => a.GetPageAssetManager<ScriptModule>())
                       .Returns(manager.Object);

            Assets.Scripts.ShouldEqual(manager.Object);
        }

        [Fact]
        public void StylesheetsReturnsPageAssetManager()
        {
            var application = new Mock<ICassetteApplication>();
            Assets.Application = application.Object;
            var manager = new Mock<IPageAssetManager>();
            application.Setup(a => a.GetPageAssetManager<StylesheetModule>())
                       .Returns(manager.Object);

            Assets.Stylesheets.ShouldEqual(manager.Object);
        }

        [Fact]
        public void HtmlTemplatesReturnsPageAssetManager()
        {
            var application = new Mock<ICassetteApplication>();
            Assets.Application = application.Object;
            var manager = new Mock<IPageAssetManager>();
            application.Setup(a => a.GetPageAssetManager<HtmlTemplateModule>())
                       .Returns(manager.Object);

            Assets.HtmlTemplates.ShouldEqual(manager.Object);
        }

    }
}
