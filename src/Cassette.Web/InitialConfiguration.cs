using System.IO.IsolatedStorage;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.Web
{
    class InitialConfiguration : ICassetteConfiguration
    {
        readonly CassetteConfigurationSection configurationSection;
        readonly bool globalIsDebuggingEnabled;
        readonly string sourceDirectory;
        readonly string virtualDirectory;
        readonly IsolatedStorageFile storage;

        public InitialConfiguration(CassetteConfigurationSection configurationSection, bool globalIsDebuggingEnabled, string sourceDirectory, string virtualDirectory, IsolatedStorageFile storage)
        {
            this.configurationSection = configurationSection;
            this.globalIsDebuggingEnabled = globalIsDebuggingEnabled;
            this.sourceDirectory = sourceDirectory;
            this.virtualDirectory = virtualDirectory;
            this.storage = storage;
        }

        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsDebuggingEnabled = 
                configurationSection.Debug.HasValue && configurationSection.Debug.Value ||
               (!configurationSection.Debug.HasValue && globalIsDebuggingEnabled);

            settings.IsHtmlRewritingEnabled = configurationSection.RewriteHtml;

            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new IsolatedStorageDirectory(storage);
            settings.UrlModifier = new VirtualDirectoryPrepender(virtualDirectory);
        }
    }
}