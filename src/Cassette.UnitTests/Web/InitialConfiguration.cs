using System;
using System.IO.IsolatedStorage;
using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class InitialConfiguration_Tests : IDisposable
    {
        readonly IsolatedStorageFile storage;
        readonly CassetteConfigurationSection section;
        readonly CassetteSettings settings;

        public InitialConfiguration_Tests()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            section = new CassetteConfigurationSection();
            settings = new CassetteSettings("");
        }

        [Fact]
        public void GivenSectionDebugNullAndGlobalDebugFalse_WhenConfigure_ThenIsDebuggedEnabledIsFalse()
        {
            section.Debug = null;
            var config = new InitialConfiguration(section, false, "/", "/");
            
            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeFalse();
        }

        [Fact]
        public void GivenSectionDebugNullAndGlobalDebugTrue_WhenConfigure_ThenIsDebuggedEnabledIsTrue()
        {
            section.Debug = null;
            var config = new InitialConfiguration(section, true, "/", "/");

            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeTrue();
        }

        [Fact]
        public void GivenSectionDebugFalseAndGlobalDebugTrue_WhenConfigure_ThenIsDebuggedEnabledIsFalse()
        {
            section.Debug = false;
            var config = new InitialConfiguration(section, true, "/", "/");

            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeFalse();
        }
        
        [Fact]
        public void GivenSectionDebugTrueAndGlobalDebugFalse_WhenConfigure_ThenIsDebuggedEnabledIsTrue()
        {
            section.Debug = true;
            var config = new InitialConfiguration(section, true, "/", "/");

            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeTrue();
        }

        public void Dispose()
        {
            storage.Dispose();
        }
    }
}

