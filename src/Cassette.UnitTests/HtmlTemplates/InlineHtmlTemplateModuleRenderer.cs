using System.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class InlineHtmlTemplateModuleRenderer_Tests
    {
        [Fact]
        public void GivenAssetInSubDirectory_WhenRender_ThenScriptIdHasSlashesReplacedWithDashes()
        {
            var module = new HtmlTemplateModule("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/sub/asset.htm");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            module.Assets.Add(asset.Object);

            var renderer = new InlineHtmlTemplateModuleRenderer();
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldContain("id=\"sub-asset\"");
        }
    }
}