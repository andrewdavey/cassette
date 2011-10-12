using System;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class ConfigurableCassetteApplication_Tests
    {
        readonly ConfigurableCassetteApplication application = new ConfigurableCassetteApplication();

        [Fact]
        public void BundlesIsAssigned()
        {
            application.Bundles.ShouldNotBeNull();
        }

        [Fact]
        public void SettingsIsAssigned()
        {
            application.Settings.ShouldNotBeNull();
        }

        [Fact]
        public void ServicesIsAssigned()
        {
            application.Services.ShouldNotBeNull();
        }

        [Fact]
        public void WhenSetSettingsToNull_ThenExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () => application.Settings = null
            );
        }

        [Fact]
        public void WhenSetServicesToNull_ThenExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () => application.Services = null
            );
        }
    }
}
