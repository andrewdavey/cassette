using System.Collections.Generic;
using System.IO;
using Cassette.Configuration;

namespace Cassette
{
    abstract class CassetteApplicationContainerFactoryBase<T>
        where T : ICassetteApplication
    {
        readonly ICassetteConfigurationFactory cassetteConfigurationFactory;
        readonly CassetteConfigurationSection configurationSection;
        readonly string physicalDirectory;
        readonly string virtualDirectory;
        readonly object creationLock = new object();
        IEnumerable<ICassetteConfiguration> cassetteConfigurations;

        protected CassetteApplicationContainerFactoryBase(ICassetteConfigurationFactory cassetteConfigurationFactory, CassetteConfigurationSection configurationSection, string physicalDirectory, string virtualDirectory)
        {
            this.cassetteConfigurationFactory = cassetteConfigurationFactory;
            this.configurationSection = configurationSection;
            this.physicalDirectory = physicalDirectory;
            this.virtualDirectory = virtualDirectory;
        }

        protected abstract bool ShouldWatchFileSystem { get; }

        protected abstract string PhysicalApplicationDirectory { get; }

        protected abstract string GetConfigurationVersion();

        protected abstract T CreateCassetteApplicationCore(IBundleContainer bundleContainer, CassetteSettings settings);

        public virtual CassetteApplicationContainer<T> CreateContainer()
        {
            cassetteConfigurations = CreateCassetteConfigurations();
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
            var container = new CassetteApplicationContainer<T>(CreateApplication);
            if (ShouldWatchFileSystem)
            {
                container.CreateNewApplicationWhenFileSystemChanges(PhysicalApplicationDirectory);
            }
            return container;
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
                var bundleContainerFactory = settings.GetBundleContainerFactory(CassetteConfigurations);
                var bundleContainer = bundleContainerFactory.CreateBundleContainer();

                Trace.Source.TraceInformation("IsDebuggingEnabled: {0}", settings.IsDebuggingEnabled);
                Trace.Source.TraceInformation("Cache version: {0}", cacheVersion);
                Trace.Source.TraceInformation("Creating Cassette application object");

                return CreateCassetteApplicationCore(bundleContainer, settings);
            }
        }

        protected IEnumerable<ICassetteConfiguration> CassetteConfigurations
        {
            get { return cassetteConfigurations; }
        }

        CassetteApplicationContainer<T> CreateContainerFromCompileTimeManifest()
        {
            var settings = CreateSettingsForBundlesFromCompileTime();
            foreach (var configuration in CassetteConfigurations)
            {
                configuration.Configure(null, settings);
            }
            var bundleContainerFactory = new CompileTimeManifestBundleContainerFactory(PrecompiledManifestFilename, settings);
            var bundleContainer = bundleContainerFactory.CreateBundleContainer();

            return new CassetteApplicationContainer<T>(
                () => CreateCassetteApplicationCore(bundleContainer, settings)
            );
        }

        CassetteSettings CreateSettingsForBundlesFromCompileTime()
        {
            return new CassetteSettings("")
            {
                IsUsingPrecompiledManifest = true,
                IsHtmlRewritingEnabled = configurationSection.RewriteHtml,
                AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics
            };
        }

        string PrecompiledManifestFilename
        {
            get { return Path.Combine(physicalDirectory, configurationSection.PrecompiledManifest); }
        }
    }
}