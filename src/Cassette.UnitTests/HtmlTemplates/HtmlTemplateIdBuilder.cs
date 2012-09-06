using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateIdBuilderTests
    {
        readonly HtmlTemplateBundle bundle;
        readonly Mock<IAsset> asset;
        HtmlTemplateIdBuilder builder;

        public HtmlTemplateIdBuilderTests()
        {
            bundle = new HtmlTemplateBundle("~/bundle");
            asset = new Mock<IAsset>();
        }

        [Fact]
        public void DefaultRemovesBundlePathAndFileExtension()
        {
            GivenAssetPath("~/bundle/template.html");
            GivenIdStrategy(new HtmlTemplateIdBuilder());
            ThenIdIs("template");
        }

        [Fact]
        public void GivenIncludeBundlePathThenIdStartsWithBundlePath()
        {
            GivenAssetPath("~/bundle/template.html");
            GivenIdStrategy(new HtmlTemplateIdBuilder(includeBundlePath: true));
            ThenIdIs("bundle/template");
        }

        [Fact]
        public void GivenNotIncludeBundlePathButBundlePathDoesntMatchAssetPathThenIdIsFullAssetPath()
        {
            GivenAssetPath("~/other/template.html");
            GivenIdStrategy(new HtmlTemplateIdBuilder());
            ThenIdIs("other/template");
        }

        [Fact]
        public void GivenIncludeFileExtensionThenIdIncludesFileExtension()
        {
            GivenAssetPath("~/asset.html");
            GivenIdStrategy(new HtmlTemplateIdBuilder(includeFileExtension: true));
            ThenIdIs("asset.html");
        }

        [Fact]
        public void GivenPathSeparatorReplacementIsDash()
        {
            GivenAssetPath("~/bundle/asset.html");
            GivenIdStrategy(new HtmlTemplateIdBuilder(pathSeparatorReplacement: "-", includeBundlePath: true));
            ThenIdIs("bundle-asset");
        }

        void GivenAssetPath(string path)
        {
            asset.SetupGet(a => a.Path).Returns(path);
        }

        void GivenIdStrategy(HtmlTemplateIdBuilder strategy)
        {
            this.builder = strategy;
        }

        void ThenIdIs(string expectedId)
        {
            var id = builder.HtmlTemplateId(bundle, asset.Object);
            id.ShouldEqual(expectedId);
        }
    }
}
