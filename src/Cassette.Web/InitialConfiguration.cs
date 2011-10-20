using System.IO.IsolatedStorage;
using System.Web;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.Web
{
    class InitialConfiguration : ICassetteConfiguration
    {
        readonly string sourceDirectory;
        readonly IsolatedStorageFile storage;
        readonly bool isDebuggingEnabled;

        public InitialConfiguration(string sourceDirectory, IsolatedStorageFile storage, bool isDebuggingEnabled)
        {
            this.sourceDirectory = sourceDirectory;
            this.storage = storage;
            this.isDebuggingEnabled = isDebuggingEnabled;
        }

        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsHtmlRewritingEnabled = true;
            settings.IsDebuggingEnabled = isDebuggingEnabled;
            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new IsolatedStorageDirectory(storage);
            settings.UrlModifier = new VirtualDirectoryPrepender(HttpRuntime.AppDomainAppVirtualPath);
        }
    }
}