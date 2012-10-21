using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplateInScriptElement_Tests
    {
        readonly HtmlTemplateBundle bundle;
        readonly Mock<IAsset> asset;
        string templateContent = "";

        public WrapHtmlTemplateInScriptElement_Tests()
        {
            bundle = new HtmlTemplateBundle("~/test");
            
            asset = new Mock<IAsset>();
            asset.Setup(a => a.Path).Returns("~/test/asset.htm");
            bundle.Assets.Add(asset.Object);
        }

        [Fact]
        public void WhenTransform_ThenScriptIdAttributeIsDerivedFromFileName()
        {
            var html = TransformToHtml();

            html.ShouldStartWith("<script id=\"asset\"");
        }

        [Fact]
        public void WhenTransform_ThenScriptTypeAttributeIsBundleContentType()
        {
            bundle.ContentType = "TEST-CONTENT-TYPE";

            var html = TransformToHtml();

            html.ShouldStartWith("<script id=\"asset\" type=\"TEST-CONTENT-TYPE\"");
        }

        [Fact]
        public void GivenTemplateContent_WhenTransform_ThenScriptContainsTemplateContent()
        {
            templateContent = "TEMPLATE";

            var html = TransformToHtml();

            html.ShouldEqual("<script id=\"asset\" type=\"text/html\">TEMPLATE</script>");
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenTransform_ThenScriptIdHasSlashesReplacedWithDashes()
        {
            asset.Setup(a => a.Path).Returns("~/test/sub/asset.htm");

            var html = TransformToHtml();

            html.ShouldContain("id=\"sub-asset\"");
        }

        [Fact]
        public void GivenBundleWithHtmlAttributes_WhenTransform_ThenTemplateScriptElementHasTheExtraAttributes()
        {
            bundle.HtmlAttributes.Add("class", "test");

            var html = TransformToHtml();

            html.ShouldContain(" class=\"test\"");
        }

        string TransformToHtml()
        {
            var transformer = new WrapHtmlTemplateInScriptElement(bundle, new DefaultHtmlTemplateIdStrategy(pathSeparatorReplacement: "-"));
            var getResult = transformer.Transform(() => templateContent.AsStream(), asset.Object);
            var html = getResult().ReadToEnd();
            return html;
        }
    }
}