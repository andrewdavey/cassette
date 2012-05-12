using System;
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
        readonly string binDirectory;
        readonly string outputDirectory;

        public MSBuildHost(string inputDirectory, string binDirectory, string outputDirectory)
        {
            if (!Path.IsPathRooted(inputDirectory)) throw new ArgumentException("inputDirectory must be an absolute path.", "inputDirectory");
            if (!Path.IsPathRooted(binDirectory)) throw new ArgumentException("binDirectory must be an absolute path.", "binDirectory");
            if (!Path.IsPathRooted(outputDirectory)) throw new ArgumentException("outputDirectory must be an absolute path.", "outputDirectory");

            this.inputDirectory = inputDirectory;
            this.binDirectory = binDirectory;
            this.outputDirectory = outputDirectory;
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return Directory
                .GetFiles(binDirectory, "*.dll")
                .Select(TryLoadAssembly)
                .Where(assembly => assembly != null);
        }

        Assembly TryLoadAssembly(string assemblyFilename)
        {
            try
            {
                return Assembly.LoadFrom(assemblyFilename);
            }
            catch
            {
                return null;
            }
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return false; }
        }

        protected override void RegisterBundleCollectionInitializer()
        {
            Container.Register<IBundleCollectionInitializer, BundleCollectionInitializer>();
            Container.Register<IBundleCollectionCache>((c, p) =>
            {
                var cacheDirectory = new FileSystemDirectory(Path.GetFullPath(outputDirectory));
                return new BundleCollectionCache(
                    cacheDirectory,
                    bundleTypeName => ResolveBundleDeserializer(bundleTypeName, c)
                );
            });
        }

        public void Execute()
        {
            var bundles = Container.Resolve<BundleCollection>();
            var cache = Container.Resolve<IBundleCollectionCache>();
            cache.Write(Manifest.Static(bundles));
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