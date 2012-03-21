using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSettings_ModifyDefaultsOfScriptBundle
    {
        [Fact]
        public void WhenModifyDefaults_ThenADefaultsObjectIsPassedToTheGivenDelegate()
        {
            var settings = new CassetteSettings("");
            BundleDefaults<ScriptBundle> defaultsPassed = null;
            settings.ModifyDefaults<ScriptBundle>(defaults =>
            {
                defaultsPassed = defaults;
            });
            defaultsPassed.ShouldNotBeNull();
        }
    }

    public class CassetteSettings_ModifyDefaultsOfStylesheetBundle
    {
        [Fact]
        public void WhenModifyDefaults_ThenADefaultsObjectIsPassedToTheGivenDelegate()
        {
            var settings = new CassetteSettings("");
            BundleDefaults<StylesheetBundle> defaultsPassed = null;
            settings.ModifyDefaults<StylesheetBundle>(defaults =>
            {
                defaultsPassed = defaults;
            });
            defaultsPassed.ShouldNotBeNull();
        }
    }

    public class CassetteSettings_ModifyDefaultsOfHtmlTemplateBundle
    {
        [Fact]
        public void WhenModifyDefaults_ThenADefaultsObjectIsPassedToTheGivenDelegate()
        {
            var settings = new CassetteSettings("");
            BundleDefaults<HtmlTemplateBundle> defaultsPassed = null;
            settings.ModifyDefaults<HtmlTemplateBundle>(defaults =>
            {
                defaultsPassed = defaults;
            });
            defaultsPassed.ShouldNotBeNull();
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
}