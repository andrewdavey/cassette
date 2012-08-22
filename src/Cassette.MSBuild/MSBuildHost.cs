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
        readonly string sourceDirectory;
        readonly string binDirectory;
        readonly string outputDirectory;
        readonly bool includeRawFiles;

        public MSBuildHost(string sourceDirectory, string binDirectory, string outputDirectory, bool includeRawFiles)
        {
            if (!Path.IsPathRooted(sourceDirectory)) throw new ArgumentException("sourceDirectory must be an absolute path.", "sourceDirectory");
            if (!Path.IsPathRooted(binDirectory)) throw new ArgumentException("binDirectory must be an absolute path.", "binDirectory");
            if (!Path.IsPathRooted(outputDirectory)) throw new ArgumentException("outputDirectory must be an absolute path.", "outputDirectory");

            this.sourceDirectory = sourceDirectory;
            this.binDirectory = binDirectory;
            this.outputDirectory = outputDirectory;
            this.includeRawFiles = includeRawFiles;
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
            WriteCache(bundles);
            if (includeRawFiles)
            {
                CopyRawFileToOutputDirectory(bundles);
            }
        }

        void WriteCache(IEnumerable<Bundle> bundles)
        {
            var cache = Container.Resolve<IBundleCollectionCache>();
            cache.Write(Manifest.Static(bundles));
        }

        void CopyRawFileToOutputDirectory(BundleCollection bundles)
        {
            bundles.Accept(new RawFileCopier(sourceDirectory, outputDirectory));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // Override any URL modifier, even if set by the application.
            // So this is *after* the base.RegisterContainerItems() call.
            // We must output specially wrapped URLs at compile-time. These are then modified by the application at run-time.
            Container.Register<IUrlModifier>(new UrlPlaceholderWrapper());
            Container.Register<IApplicationRootPrepender>(new ApplicationRootPlaceholderWrapper());
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new MsBuildHostSettingsConfiguration(sourceDirectory);
        }
    }
}