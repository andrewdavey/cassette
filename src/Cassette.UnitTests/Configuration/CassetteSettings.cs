using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSettings_DefaultBundleProcessor_Tests
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
        public void
            GivenSettingsSetDefaultBundleProcessorForScriptBundle_WhenGetDefaultBundleProcessorForScriptBundle_ThenReturnTheProcessor
            ()
        {
            var settings = new CassetteSettings("");
            var processor = Mock.Of<IBundleProcessor<ScriptBundle>>();

            settings.SetDefaultBundleProcessor(processor);

            settings.GetDefaultBundleProcessor<ScriptBundle>().ShouldBeSameAs(processor);
        }
    }

    public class CassetteSettings_RequestRawFile_Tests
    {
        [Fact]
        public void ByDefaultDenyAllRawFileRequests()
        {
            var settings = new CassetteSettings("");
            settings.CanRequestRawFile("~/file.png").ShouldBeFalse();
        }

        [Fact]
        public void GivenAllowRawFileRequestPredicate_ThenAllowRequest()
        {
            var settings = new CassetteSettings("");
            settings.AllowRawFileRequest(path => path == "~/file.png");
            settings.CanRequestRawFile("~/file.png").ShouldBeTrue();
        }
    }

    public class CassetteSettings_DefaultFileSearch_Tests
    {
        readonly CassetteSettings settings = new CassetteSettings("");

        [Fact]
        public void DefaultFileSearchOfScriptBundleMatchesJsFile()
        {
            AssertMatches("~/test.js", settings.GetDefaultFileSearch<ScriptBundle>());
        }

        [Fact]
        public void DefaultFileSearchOfStylesheetBundleMatchesCssFile()
        {
            AssertMatches("~/test.css", settings.GetDefaultFileSearch<StylesheetBundle>());
        }

        [Fact]
        public void DefaultFileSearchOfHtmlTemplateBundleMatchesHtmFile()
        {
            AssertMatches("~/test.htm", settings.GetDefaultFileSearch<HtmlTemplateBundle>());
        }

        [Fact]
        public void DefaultFileSearchOfHtmlTemplateBundleMatchesHtmlFile()
        {
            AssertMatches("~/test.html", settings.GetDefaultFileSearch<HtmlTemplateBundle>());
        }

        [Fact]
        public void GivenCoffeeScriptFileSearchPatternAddedForScriptBundle_ThenDefaultFileSearchMatchCoffeeFile()
        {
            settings.AddDefaultFileSearchPattern<ScriptBundle>("*.coffee");
            AssertMatches("~/test.coffee", settings.GetDefaultFileSearch<ScriptBundle>());
        }

        [Fact]
        public void GivenADefaultFileSearchIsAssigned_ThenGetDefaultFileSearchReturnsIt()
        {
            var fileSearch = new FileSearch();
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSearch;
            settings.GetDefaultFileSearch<ScriptBundle>().ShouldBeSameAs(fileSearch);
        }
        
        void AssertMatches(string filename, IFileSearch fileSearch)
        {
            var directory = new FakeFileSystem { filename };
            fileSearch.FindFiles(directory).ShouldNotBeEmpty();
        }
    }
}