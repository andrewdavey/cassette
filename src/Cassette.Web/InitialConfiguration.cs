using System.IO;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.Web
{
    class InitialConfiguration : ICassetteConfiguration
    {
        readonly CassetteConfigurationSection configurationSection;
        readonly bool isAspNetDebuggingEnabled;
        readonly string sourceDirectory;
        readonly string virtualDirectory;

        public InitialConfiguration(CassetteConfigurationSection configurationSection, bool isAspNetDebuggingEnabled, string sourceDirectory, string virtualDirectory)
        {
            this.configurationSection = configurationSection;
            this.isAspNetDebuggingEnabled = isAspNetDebuggingEnabled;
            this.sourceDirectory = sourceDirectory;
            this.virtualDirectory = virtualDirectory;
        }

        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsDebuggingEnabled = configurationSection.Debug.HasValue ? configurationSection.Debug.Value : isAspNetDebuggingEnabled;
            settings.IsHtmlRewritingEnabled = configurationSection.RewriteHtml;
            settings.AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics;
            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);

            //TODO: Decide if this is the right place to put sprites

            var spriteDirectory = Path.Combine(sourceDirectory, "Sprites");
            settings.SpriteDirectory = new FileSystemDirectory(spriteDirectory);

            settings.UrlModifier = new VirtualDirectoryPrepender(virtualDirectory);
        }
    }
}