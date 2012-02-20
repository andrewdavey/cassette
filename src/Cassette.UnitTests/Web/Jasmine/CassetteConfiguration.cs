using Cassette.Configuration;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Should;
using Xunit;
using System;
using System.IO;

namespace Cassette.Web.Jasmine
{
    public class CassetteConfiguration_WhenConfigure : IDisposable
    {
        readonly BundleCollection bundles;
        readonly TempDirectory path;

        public CassetteConfiguration_WhenConfigure()
        {
            path = new TempDirectory();
            Directory.CreateDirectory(Path.Combine(path, "scripts", "jasmine"));
            File.WriteAllText(Path.Combine(path, "scripts", "jasmine", "jasmine.js"), "");
            File.WriteAllText(Path.Combine(path, "scripts", "jasmine", "jasmine.css"), "");

            var config = new CassetteConfiguration();
            var settings = new CassetteSettings("")
            {
                SourceDirectory = new FileSystemDirectory(path)
            };
            bundles = new BundleCollection(settings);

            config.Configure(bundles, settings);
        }

        [Fact]
        public void ThenScriptBundleAddedToBundles()
        {
            bundles.Get<ScriptBundle>("scripts/jasmine").ShouldNotBeNull();
        }

        [Fact]
        public void ThenStylesheetBundleAddedToBundles()
        {
            bundles.Get<StylesheetBundle>("scripts/jasmine").ShouldNotBeNull();
        }

        public void Dispose()
        {
            path.Dispose();
        }
    }
}