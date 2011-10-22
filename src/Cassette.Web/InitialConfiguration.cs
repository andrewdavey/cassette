using System.IO.IsolatedStorage;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.Web
{
    class InitialConfiguration : ICassetteConfiguration
    {
        readonly string sourceDirectory;
        readonly string virtualDirectory;
        readonly IsolatedStorageFile storage;
        readonly bool isDebuggingEnabled;

        public InitialConfiguration(string sourceDirectory, string virtualDirectory, IsolatedStorageFile storage, bool isDebuggingEnabled)
        {
            this.sourceDirectory = sourceDirectory;
            this.virtualDirectory = virtualDirectory;
            this.storage = storage;
            this.isDebuggingEnabled = isDebuggingEnabled;
        }

        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsHtmlRewritingEnabled = true;
            settings.IsDebuggingEnabled = isDebuggingEnabled;
            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new IsolatedStorageDirectory(storage);
            settings.UrlModifier = new VirtualDirectoryPrepender(virtualDirectory);
        }
    }
}