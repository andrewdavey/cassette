using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cassette.Caching;
using Cassette.IO;

namespace Cassette.MSBuild
{
    public class MSBuildHost : HostBase
    {
        readonly string inputDirectory;
        readonly string outputDirectory;

        public MSBuildHost(string inputDirectory, string outputDirectory)
        {
            this.inputDirectory = inputDirectory;
            this.outputDirectory = outputDirectory;
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return Directory
                .GetFiles(inputDirectory, "*.dll")
                .Select(Assembly.LoadFrom);
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return false; }
        }

        protected override void RegisterBundleCollectionInitializer()
        {
            Container.Register<IBundleCollectionInitializer, BundleCollectionInitializer>();
        }

        public void Execute()
        {
            var bundles = Container.Resolve<BundleCollection>();
            var settings = Container.Resolve<CassetteSettings>();
            var cacheDirectory = settings.SourceDirectory.GetDirectory(outputDirectory);
            WriteCache(bundles, cacheDirectory);
        }

        void WriteCache(BundleCollection bundles, IDirectory cacheDirectory)
        {
            var cache = new BundleCollectionCache(cacheDirectory);
            cache.Write(bundles);
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // Override any URL modifier, even if set by the application.
            // So this is *after* the base.RegisterContainerItems() call.
            // We must output specially wrapped URLs at compile-time. These are then modified by the application at run-time.
            Container.Register<IUrlModifier>(new UrlPlaceholderWrapper());
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new MsBuildHostSettingsConfiguration();
        }
    }
}