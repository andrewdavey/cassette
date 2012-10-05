using System.IO;
using System.Text;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateToJavaScriptTransformerTests
    {
        [Fact]
        public void TemplateTransformedIntoAddTemplateJavaScriptCall()
        {
            GivenAssetPath("~/bundle/asset.html");
            WhenTransform("<p class=\"example\">template</p>");
            ThenOutputIs("addTemplate(\"bundle/asset\",\"<p class=\\\"example\\\">template</p>\");");
        }

        [Fact]
        public void AssetPathWithoutExtension()
        {
            GivenAssetPath("~/bundle/asset");
            WhenTransform("<p class=\"example\">template</p>");
            ThenOutputIs("addTemplate(\"bundle/asset\",\"<p class=\\\"example\\\">template</p>\");");
        }

        [Fact]
        public void AssetPathWithoutExtensionButPeriodInDirectoryName()
        {
            GivenAssetPath("~/bundle.test/asset");
            WhenTransform("<p class=\"example\">template</p>");
            ThenOutputIs("addTemplate(\"bundle.test/asset\",\"<p class=\\\"example\\\">template</p>\");");
        }

        [Fact]
        public void IdStrategyUsed()
        {
            var strategy = new Mock<IHtmlTemplateIdStrategy>();

            GivenAssetPath("~/bundle/asset.html");
            GivenIdStrategy(strategy.Object);
            WhenTransform("");

            strategy.Verify(s => s.HtmlTemplateId(bundle, asset.Object));
        }

        readonly Mock<IAsset> asset;
        readonly HtmlTemplateBundle bundle;
        string transformOutput;
        IHtmlTemplateIdStrategy htmlTemplateIdStrategy;
        readonly IHtmlTemplateScriptStrategy scriptStrategy;

        public HtmlTemplateToJavaScriptTransformerTests()
        {
            asset = new Mock<IAsset>();
            htmlTemplateIdStrategy = new HtmlTemplateIdBuilder();
            scriptStrategy = new DomHtmlTemplateScriptStrategy(new SimpleJsonSerializer());
            bundle = new HtmlTemplateBundle("~");
        }

        void GivenAssetPath(string path)
        {
            asset.SetupGet(a => a.Path).Returns(path);
        }

        void GivenIdStrategy(IHtmlTemplateIdStrategy htmlTemplateIdStrategy)
        {
            this.htmlTemplateIdStrategy = htmlTemplateIdStrategy;
        }

        void WhenTransform(string htmlTemplate)
        {
            var transformer = new HtmlTemplateToJavaScriptTransformer(
                bundle,
                scriptStrategy,
                htmlTemplateIdStrategy
            );

            var getOutput = transformer.Transform(
                () => new MemoryStream(Encoding.UTF8.GetBytes(htmlTemplate)),
                asset.Object
            );

            using (var reader = new StreamReader(getOutput()))
            {
                transformOutput = reader.ReadToEnd();
            }
        }

        void ThenOutputIs(string expected)
        {
            transformOutput.ShouldEqual(expected);
        }
    }
}