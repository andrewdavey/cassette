using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class LocalAssetSettings_Defaults_Tests
    {
        readonly LocalAssetSettings settings;

        public LocalAssetSettings_Defaults_Tests()
        {
            settings = new LocalAssetSettings();            
        }

        [Fact]
        public void PathIsRoot()
        {
            settings.Path.ShouldEqual("~/");
        }

        [Fact]
        public void FallbackConditionIsNull()
        {
            settings.FallbackCondition.ShouldBeNull();
        }

        [Fact]
        public void UseWhenDebuggingIsTrue()
        {
            settings.UseWhenDebugging.ShouldBeTrue();
        }

        [Fact]
        public void FileSearchIsNull()
        {
            settings.FileSearch.ShouldBeNull();
        }
    }
}