using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetHtmlRenderer_Tests
    {
        readonly Mock<ICassetteApplication> application;
        readonly Mock<IBundleHtmlRenderer<StylesheetBundle>> fallbackRenderer;
        readonly ExternalStylesheetHtmlRenderer renderer;
        readonly ExternalStylesheetBundle bundle;

        public ExternalStylesheetHtmlRenderer_Tests()
        {
            application = new Mock<ICassetteApplication>();
            fallbackRenderer = new Mock<IBundleHtmlRenderer<StylesheetBundle>>(); 
            renderer = new ExternalStylesheetHtmlRenderer(fallbackRenderer.Object, application.Object);
            bundle = new ExternalStylesheetBundle("http://test.com/");
        }

        [Fact]
        public void GivenApplicationInProduction_WhenRender_ThenLinkElementReturnedWithBundleUrlAsHref()
        {
            application.SetupGet(a => a.IsDebuggingEnabled).Returns(false);
            
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\"/>");
        }

        [Fact]
        public void GivenApplicationInProduction_WhenRenderBundleWithMedia_ThenLinkElementReturnedWithMediaAttribute()
        {
            application.SetupGet(a => a.IsDebuggingEnabled).Returns(false);
            bundle.Media = "print";

            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>");
        }

        [Fact]
        public void GivenApplicationInDebugMode_WhenRenderBundleWithAssets_ThenFallbackRendererIsUsed()
        {
            application.SetupGet(a => a.IsDebuggingEnabled).Returns(true);
            bundle.Assets.Add(Mock.Of<IAsset>());

            renderer.Render(bundle);

            fallbackRenderer.Verify(r => r.Render(bundle));
        }
    }
}
