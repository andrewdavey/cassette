using System.Collections.Generic;
using System.IO;
using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette
{
    abstract class CassetteApplicationContainerFactoryBase<T>
        where T : ICassetteApplication
    {
        readonly ICassetteConfigurationFactory cassetteConfigurationFactory;
        readonly CassetteConfigurationSection configurationSection;
        readonly string physicalDirectory;
        readonly object creationLock = new object();
        IEnumerable<ICassetteConfiguration> cassetteConfigurations;
        BundleCollection bundles;

        protected CassetteApplicationContainerFactoryBase(ICassetteConfigurationFactory cassetteConfigurationFactory, CassetteConfigurationSection configurationSection, string physicalDirectory)
        {
            this.cassetteConfigurationFactory = cassetteConfigurationFactory;
            this.configurationSection = configurationSection;
            this.physicalDirectory = physicalDirectory;
        }

        protected abstract bool ShouldWatchFileSystem { get; }

        protected abstract string PhysicalApplicationDirectory { get; }

        protected abstract string GetConfigurationVersion();

        protected abstract T CreateCassetteApplicationCore(IBundleContainer bundleContainer, CassetteSettings settings);

        public virtual CassetteApplicationContainer<T> CreateContainer()
        {
            if (string.IsNullOrEmpty(configurationSection.PrecompiledManifest))
            {
                return CreateContainerFromConfiguration();
            }
            else
            {
                return CreateContainerFromCompileTimeManifest();
            }
        }

        CassetteApplicationContainer<T> CreateContainerFromConfiguration()
        {
            cassetteConfigurations = CreateCassetteConfigurations();
            var container = new CassetteApplicationContainer<T>(CreateApplication);
            if (ShouldWatchFileSystem)
            {
                container.CreateNewApplicationWhenFileSystemChanges(PhysicalApplicationDirectory);
            }
            return container;
        }

        CassetteApplicationContainer<T> CreateContainerFromCompileTimeManifest()
        {
            using (var file = OpenManifestFile())
            {
                var reader = new CassetteManifestReader(file);
                var manifest = reader.Read();
                var settings = new CassetteSettings("");
                bundles = new BundleCollection(settings);
                foreach (var bundle in manifest.CreateBundles())
                {
                    bundles.Add(bundle);
                }
                var bundleContainer = new BundleContainer(bundles);
                return new CassetteApplicationContainer<T>(() => CreateCassetteApplicationCore(bundleContainer, settings));
            }
        }

        FileStream OpenManifestFile()
        {
            var filename = Path.Combine(physicalDirectory, configurationSection.PrecompiledManifest);
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Cannot find the file \"{0}\" specified by precompiledManifest in the <cassette> configuration section.", filename);
            }
            return File.OpenRead(filename);
        }

        protected virtual IEnumerable<ICassetteConfiguration> CreateCassetteConfigurations()
        {
            return cassetteConfigurationFactory.CreateCassetteConfigurations();
        }

        T CreateApplication()
        {
            lock (creationLock)
            {
                var cacheVersion = GetConfigurationVersion();
                var settings = new CassetteSettings(cacheVersion);
                bundles = new BundleCollection(settings);
                ExecuteCassetteConfiguration(settings);
                ProcessBundles(settings);

                Trace.Source.TraceInformation("IsDebuggingEnabled: {0}", settings.IsDebuggingEnabled);
                Trace.Source.TraceInformation("Cache version: {0}", cacheVersion);
                Trace.Source.TraceInformation("Creating Cassette application object");

                var bundleContainer = new BundleContainer(bundles);
                return CreateCassetteApplicationCore(bundleContainer, settings);
            }
        }

        void ProcessBundles(CassetteSettings settings)
        {
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }
        }

        void ExecuteCassetteConfiguration(CassetteSettings settings)
        {
            foreach (var configuration in CassetteConfigurations)
            {
                Trace.Source.TraceInformation("Executing configuration {0}", configuration.GetType().AssemblyQualifiedName);
                configuration.Configure(bundles, settings);
            }
        }

        protected IEnumerable<ICassetteConfiguration> CassetteConfigurations
        {
            get { return cassetteConfigurations; }
        }
    }
}