using System.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplateInScriptElement_Tests
    {
        [Fact]
        public void GivenAssetInSubDirectory_WhenTransform_ThenScriptIdHasSlashesReplacedWithDashes()
        {
            var bundle = new HtmlTemplateBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFile.FullPath).Returns("~/test/sub/asset.htm");
            bundle.Assets.Add(asset.Object);

            var transformer = new WrapHtmlTemplateInScriptElement(bundle);
            var getResult = transformer.Transform(() => Stream.Null, asset.Object);
            var html = getResult().ReadToEnd();

            html.ShouldContain("id=\"sub-asset\"");
        }
    }
}