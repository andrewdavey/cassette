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

        public InitialConfiguration(CassetteConfigurationSection configurationSection, bool globalIsDebuggingEnabled, string sourceDirectory, string virtualDirectory)
        {
            this.configurationSection = configurationSection;
            this.globalIsDebuggingEnabled = globalIsDebuggingEnabled;
            this.sourceDirectory = sourceDirectory;
            this.virtualDirectory = virtualDirectory;
        }

        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsDebuggingEnabled = 
                configurationSection.Debug.HasValue && configurationSection.Debug.Value ||
               (!configurationSection.Debug.HasValue && globalIsDebuggingEnabled);

            settings.IsHtmlRewritingEnabled = configurationSection.RewriteHtml;
            settings.AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics;

            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);
            settings.UrlModifier = new VirtualDirectoryPrepender(virtualDirectory);
        }
    }
}