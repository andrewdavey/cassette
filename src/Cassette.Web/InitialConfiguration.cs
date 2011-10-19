using System.Web;
using Cassette.Configuration;
using Cassette.IO;
using System.IO.IsolatedStorage;

namespace Cassette.Web
{
    class InitialConfiguration : ICassetteConfiguration
    {
        readonly string sourceDirectory;
        readonly IsolatedStorageFile storage;

        public InitialConfiguration(string sourceDirectory, IsolatedStorageFile storage)
        {
            this.sourceDirectory = sourceDirectory;
            this.storage = storage;
        }

        public void Configure(IConfigurableCassetteApplication application)
        {
            application.Settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            application.Settings.CacheDirectory = new IsolatedStorageDirectory(storage);

            application.Services.CreateUrlModifier = () => new VirtualDirectoryPrepender(HttpRuntime.AppDomainAppVirtualPath);
        }
    }
}