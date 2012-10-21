using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class DefaultHtmlTemplateIdStrategyTests
    {
        readonly HtmlTemplateBundle bundle;
        readonly Mock<IAsset> asset;
        DefaultHtmlTemplateIdStrategy strategy;

        public DefaultHtmlTemplateIdStrategyTests()
        {
            bundle = new HtmlTemplateBundle("~/bundle");
            asset = new Mock<IAsset>();
        }

        [Fact]
        public void DefaultRemovesBundlePathAndFileExtension()
        {
            GivenAssetPath("~/bundle/template.html");
            GivenIdStrategy(new DefaultHtmlTemplateIdStrategy());
            ThenIdIs("template");
        }

        [Fact]
        public void GivenIncludeBundlePathThenIdStartsWithBundlePath()
        {
            GivenAssetPath("~/bundle/template.html");
            GivenIdStrategy(new DefaultHtmlTemplateIdStrategy(includeBundlePath: true));
            ThenIdIs("bundle/template");
        }

        [Fact]
        public void GivenNotIncludeBundlePathButBundlePathDoesntMatchAssetPathThenIdIsFullAssetPath()
        {
            GivenAssetPath("~/other/template.html");
            GivenIdStrategy(new DefaultHtmlTemplateIdStrategy());
            ThenIdIs("other/template");
        }

        [Fact]
        public void GivenIncludeFileExtensionThenIdIncludesFileExtension()
        {
            GivenAssetPath("~/asset.html");
            GivenIdStrategy(new DefaultHtmlTemplateIdStrategy(includeFileExtension: true));
            ThenIdIs("asset.html");
        }

        [Fact]
        public void GivenPathSeparatorReplacementIsDash()
        {
            GivenAssetPath("~/bundle/asset.html");
            GivenIdStrategy(new DefaultHtmlTemplateIdStrategy(pathSeparatorReplacement: "-", includeBundlePath: true));
            ThenIdIs("bundle-asset");
        }

        [Fact]
        public void GivenAssetPathIsBundlePath()
        {
            GivenAssetPath("~/bundle");
            GivenIdStrategy(new DefaultHtmlTemplateIdStrategy());
            ThenIdIs("bundle");
        }

        void GivenAssetPath(string path)
        {
            asset.SetupGet(a => a.Path).Returns(path);
        }

        void GivenIdStrategy(DefaultHtmlTemplateIdStrategy strategy)
        {
            this.strategy = strategy;
        }

        void ThenIdIs(string expectedId)
        {
            var id = strategy.GetHtmlTemplateId(asset.Object, bundle);
            id.ShouldEqual(expectedId);
        }
    }
}
