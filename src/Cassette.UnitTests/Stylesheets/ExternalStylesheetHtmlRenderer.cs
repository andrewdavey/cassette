using System;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetHtmlRenderer_Tests
    {
        readonly CassetteSettings settings;
        readonly Mock<IBundleHtmlRenderer<StylesheetBundle>> fallbackRenderer;
        readonly ExternalStylesheetHtmlRenderer renderer;
        readonly ExternalStylesheetBundle bundle;

        public ExternalStylesheetHtmlRenderer_Tests()
        {
            settings = new CassetteSettings("");
            fallbackRenderer = new Mock<IBundleHtmlRenderer<StylesheetBundle>>(); 
            renderer = new ExternalStylesheetHtmlRenderer(fallbackRenderer.Object, settings);
            bundle = new ExternalStylesheetBundle("http://test.com/");
        }

        [Fact]
        public void GivenApplicationInProduction_WhenRender_ThenLinkElementReturnedWithBundleUrlAsHref()
        {
            settings.IsDebuggingEnabled = false;
            
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\"/>");
        }

        [Fact]
        public void GivenApplicationInProdctionAndBundleHasCondition_WhenRender_ThenLinkElementReturnedWrappedWithCondition()
        {
            settings.IsDebuggingEnabled = false;
            bundle.Condition = "CONDITION";

            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine +
                "<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine +
                "<![endif]-->");
        }

        [Fact]
        public void GivenApplicationInProdctionAndBundleHasNotIECondition_WhenRender_ThenLinkElementReturnedWrappedWithConditionButLeaveLinkVisibleToAllBrowsers()
        {
            settings.IsDebuggingEnabled = false;
            bundle.Condition = "(gt IE 9)|!(IE)";

            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if " + bundle.Condition + "]>-->" + Environment.NewLine +
                "<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine +
                "<!-- <![endif]-->");
        }


        [Fact]
        public void GivenApplicationInProductionAndBundleHasHtmlAttribute_WhenRender_ThenLinkElementReturnedWithExtraAttributes()
        {
            settings.IsDebuggingEnabled = false;
            bundle.HtmlAttributes["class"] = "foo";

            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\" class=\"foo\"/>");
        }

        [Fact]
        public void GivenApplicationInProduction_WhenRenderBundleWithMedia_ThenLinkElementReturnedWithMediaAttribute()
        {
            settings.IsDebuggingEnabled = false;
            bundle.Media = "print";

            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>");
        }

        [Fact]
        public void GivenApplicationInDebugMode_WhenRenderBundleWithAssets_ThenFallbackRendererIsUsed()
        {
            settings.IsDebuggingEnabled = true;
            bundle.Assets.Add(Mock.Of<IAsset>());

            renderer.Render(bundle);

            fallbackRenderer.Verify(r => r.Render(bundle));
        }

        [Fact]
        public void GivenApplicationInDebugMode_WhenRenderBundleWithNoLocalAssets_ThenNormalLinkElementIsReturned()
        {
            settings.IsDebuggingEnabled = true;

            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\"/>");
        }
    }
}