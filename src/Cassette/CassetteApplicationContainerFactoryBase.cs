using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    abstract class CassetteApplicationContainerFactoryBase<T>
        where T : ICassetteApplication
    {
        readonly ICassetteConfigurationFactory cassetteConfigurationFactory;
        readonly object creationLock = new object();
        IEnumerable<ICassetteConfiguration> cassetteConfigurations;

        protected CassetteApplicationContainerFactoryBase(ICassetteConfigurationFactory cassetteConfigurationFactory)
        {
            this.cassetteConfigurationFactory = cassetteConfigurationFactory;
        }

        protected BundleCollection Bundles { get; private set; }
        protected CassetteSettings Settings { get; private set; }
        protected abstract bool ShouldWatchFileSystem { get; }
        protected abstract string PhysicalApplicationDirectory { get; }

        public CassetteApplicationContainer<T> CreateContainer()
        {
            cassetteConfigurations = CreateCassetteConfigurations();
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
                Settings = new CassetteSettings(cacheVersion);
                Bundles = new BundleCollection(Settings);
                ExecuteCassetteConfiguration();

                Trace.Source.TraceInformation("IsDebuggingEnabled: {0}", Settings.IsDebuggingEnabled);
                Trace.Source.TraceInformation("Cache version: {0}", cacheVersion);
                Trace.Source.TraceInformation("Creating Cassette application object");
                return CreateCassetteApplicationCore();
            }
        }

        protected abstract string GetConfigurationVersion();

        protected abstract T CreateCassetteApplicationCore();

        void ExecuteCassetteConfiguration()
        {
            foreach (var configuration in CassetteConfigurations)
            {
                Trace.Source.TraceInformation("Executing configuration {0}", configuration.GetType().AssemblyQualifiedName);
                configuration.Configure(Bundles, Settings);
            }
        }

        protected IEnumerable<ICassetteConfiguration> CassetteConfigurations
        {
            get { return cassetteConfigurations; }
        }
    }
}