using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSettings_Tests
    {
        [Fact]
        public void GetDefaultBundleProcessorForScriptBundleReturnsScriptPipeline()
        {
            var settings = new CassetteSettings("");
            var processor = settings.GetDefaultBundleProcessor<ScriptBundle>();
            processor.ShouldBeType<ScriptPipeline>();
        }

        [Fact]
        public void GetDefaultBundleProcessorForStylesheetBundleReturnsStylesheetPipeline()
        {
            var settings = new CassetteSettings("");
            var processor = settings.GetDefaultBundleProcessor<StylesheetBundle>();
            processor.ShouldBeType<StylesheetPipeline>();
        }

        [Fact]
        public void GetDefaultBundleProcessorForHtmlTemplateBundleReturnsHtmlTemplatePipeline()
        {
            var settings = new CassetteSettings("");
            var processor = settings.GetDefaultBundleProcessor<HtmlTemplateBundle>();
            processor.ShouldBeType<HtmlTemplatePipeline>();
        }

        [Fact]
        public void GivenSettingsSetDefaultBundleProcessorForScriptBundle_WhenGetDefaultBundleProcessorForScriptBundle_ThenReturnTheProcessor()
        {
            var settings = new CassetteSettings("");
            var processor = Mock.Of<IBundleProcessor<ScriptBundle>>();

            settings.SetDefaultBundleProcessor(processor);

            settings.GetDefaultBundleProcessor<ScriptBundle>().ShouldBeSameAs(processor);
        }
    }
}
