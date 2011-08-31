using Moq;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplatesInScriptBlocks_Tests
    {
        [Fact]
        public void ProcessAddsTransformerToEachAsset()
        {
            var module = new HtmlTemplateModule("~");
            var asset = new Mock<IAsset>();
            module.Assets.Add(asset.Object);

            new WrapHtmlTemplatesInScriptBlocks().Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                transformer => transformer is WrapHtmlTemplateInScriptBlock
            )));
        }
    }
}