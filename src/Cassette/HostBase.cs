using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;
using TinyIoC;
#if NET35
using System.Reflection.Emit;
#endif

namespace Cassette
{
    /// <summary>
    /// A host initializes Cassette for an application.
    /// </summary>
    public abstract class HostBase : IDisposable
    {
        TinyIoCContainer container;
        Type[] allTypes;
        Type[] configurationTypes;
        AssemblyScanner assemblyScanner;
        CassetteSettings settings;
        readonly object createSettingsLock = new object();

        public void Initialize()
        {
            var assemblies = LoadAssemblies();
            assemblyScanner = new AssemblyScanner(assemblies);
            allTypes = assemblyScanner.GetAllTypes();
            configurationTypes = GetConfigurationTypes(allTypes).ToArray();

            container = new TinyIoCContainer();
            ConfigureContainer();
            RunStartUpTasks();
            InitializeBundles();
        }

        protected TinyIoCContainer Container
        {
            get
            {
                if (container == null) throw new InvalidOperationException("Container is not available before Initialize has been called.");
                return container;
            }
        }

        /// <summary>
        /// Loads and returns all the assemblies used by the application. These will be scanned for Cassette types.
        /// </summary>
        protected abstract IEnumerable<Assembly> LoadAssemblies();

        /// <summary>
        /// Returns all types that implement <see cref="IConfiguration{T}"/>.
        /// </summary>
        protected virtual IEnumerable<Type> GetConfigurationTypes(IEnumerable<Type> typesToSearch)
        {
            var publicTypes =
                from type in typesToSearch
                where type.IsClass && !type.IsAbstract
                from interfaceType in type.GetInterfaces()
                where interfaceType.IsGenericType &&
                      interfaceType.GetGenericTypeDefinition() == typeof(IConfiguration<>)
                select type;

            var internalTypes = new[]
            {
                typeof(ScriptContainersConfiguration),
                typeof(StylesheetsContainerConfiguration),
                typeof(HtmlTemplatesContainerConfiguration)
            };

            return publicTypes.Concat(internalTypes);
        }

        protected virtual void ConfigureContainer()
        {
            // REGISTER ALL THE THINGS!
            RegisterBundleCollection();
            RegisterUrlGenerator();
            RegisterManifestCache();
            RegisterBundleCollectionInitializer();
            RegisterStartUpTasks();
            RegisterSettings();
            RegisterBundleFactoryProvider();
            RegisterFileSearchProvider();
            RegisterPerRequestServices();
            RegisterConfigurationTypes();

            // Classes that implement IConfiguration<TinyIoCContainer> can register services in the container.
            // This means plugins and the application can add to and override Cassette's default services.
            ExecuteContainerConfigurations();
        }

        void RegisterBundleCollection()
        {
            container.Register(typeof(BundleCollection)).AsSingleton();
        }

        void RegisterUrlGenerator()
        {
            container.Register(typeof(IUrlGenerator), typeof(UrlGenerator));
        }

        void RegisterManifestCache()
        {
            container.Register(typeof(ICassetteManifestCache), (c, p) =>
            {
                var cacheFile = c.Resolve<CassetteSettings>().CacheDirectory.GetFile("cassette.xml");
                return new CassetteManifestCache(cacheFile);
            });
        }

        void RegisterBundleCollectionInitializer()
        {
            container.Register(typeof(IBundleCollectionInitializer), BundleCollectionInitializerType);
            container.Register(typeof(PrecompiledBundleCollectionInitializer), (c, p) =>
            {
                var file = container.Resolve<CassetteSettings>().PrecompiledManifestFile;
                return new PrecompiledBundleCollectionInitializer(file, c.Resolve<IUrlModifier>());
            });
        }

        void RegisterStartUpTasks()
        {
            container.RegisterMultiple(typeof(IStartUpTask), GetStartUpTaskTypes());
        }

        void RegisterSettings()
        {
            container.Register(typeof(CassetteSettings), (c, p) => CreateSettings(c));
        }

        void RegisterBundleFactoryProvider()
        {
            container.Register(
                typeof(IBundleFactoryProvider),
                (c, p) => new BundleFactoryProvider(
                    bundleType =>
                    {
                        var factoryType = typeof(IBundleFactory<>).MakeGenericType(bundleType);
                        return (IBundleFactory<Bundle>)c.Resolve(factoryType);
                    }
                )
            );
        }

        void RegisterFileSearchProvider()
        {
            container.Register(
                typeof(IFileSearchProvider),
                (c, p) => new FileSearchProvider(
                    bundleType => c.Resolve<IFileSearch>(FileSearchComponentName(bundleType))
                )
            );
        }

        void RegisterPerRequestServices()
        {
            if (!CanCreateRequestLifetimeProvider) return;

            container
                .Register(typeof(IReferenceBuilder), typeof(ReferenceBuilder))
                .AsPerRequestSingleton(CreateRequestLifetimeProvider());

            container
                .Register(typeof(PlaceholderTracker))
                .AsPerRequestSingleton(CreateRequestLifetimeProvider());

            container.
                Register(typeof(IPlaceholderTracker), (c, p) => CreatePlaceholderTracker(c));
        }

        protected abstract bool CanCreateRequestLifetimeProvider { get; }

        protected virtual TinyIoCContainer.ITinyIoCObjectLifetimeProvider CreateRequestLifetimeProvider()
        {
            throw new NotSupportedException();
        }

        void ExecuteContainerConfigurations()
        {
            var containerConfigurations = CreateContainerConfigurations();
            foreach (var containerConfiguration in containerConfigurations)
            {
                containerConfiguration.Configure(container);
            }
        }

        IEnumerable<IConfiguration<TinyIoCContainer>> CreateContainerConfigurations()
        {
            return
                from type in configurationTypes
                where typeof(IConfiguration<TinyIoCContainer>).IsAssignableFrom(type)
                orderby ConfigurationOrderAttribute.GetOrder(type)
                select CreateContainerConfiguration(type);
        }

        IConfiguration<TinyIoCContainer> CreateContainerConfiguration(Type configurationC)
        {
            var ctor = configurationC.GetConstructors().First();
            var isDefaultConstructor = ctor.GetParameters().Length == 0;
            if (isDefaultConstructor)
            {
                return (IConfiguration<TinyIoCContainer>)Activator.CreateInstance(configurationC);
            }
            else
            {
                // Special hack for the bundle container configurations.
                // They take the GetImplementationTypes delegate.
                var args = new object[] { new Func<Type, IEnumerable<Type>>(GetImplementationTypes) };
                return (IConfiguration<TinyIoCContainer>)ctor.Invoke(args);
            }
        }

        void RegisterConfigurationTypes()
        {
            var configurations =
                from type in configurationTypes
                from interfaceType in type.GetInterfaces()
                where interfaceType.IsGenericType &&
                      interfaceType.GetGenericTypeDefinition() == typeof(IConfiguration<>) &&
                      interfaceType != typeof(IConfiguration<TinyIoCContainer>) //  These have already been created and used.
                select new
                {
                    registrationType = interfaceType,
                    implementationType = type
                };

            var groupedByRegistrationType = configurations.GroupBy(
                c => c.registrationType,
                c => c.implementationType
            );
            foreach (var configs in groupedByRegistrationType)
            {
                var registrationType = configs.Key;
                var implementationTypes = configs.OrderBy(ConfigurationOrderAttribute.GetOrder);
                container.RegisterMultiple(registrationType, implementationTypes);
            }
        }

        IEnumerable<Type> GetImplementationTypes(Type baseType)
        {
            return allTypes.Where(baseType.IsAssignableFrom); 
        }

        protected virtual Type BundleCollectionInitializerType
        {
            get { return typeof(RuntimeBundleCollectionInitializer); }
        }

        IPlaceholderTracker CreatePlaceholderTracker(TinyIoCContainer currentContainer)
        {
            if (currentContainer.Resolve<CassetteSettings>().IsHtmlRewritingEnabled)
            {
                return currentContainer.Resolve<PlaceholderTracker>();
            }
            else
            {
                return new NullPlaceholderTracker();
            }
        }

        protected virtual IEnumerable<Type> GetStartUpTaskTypes()
        {
            return GetImplementationTypes(typeof(IStartUpTask));
        }

        void InitializeBundles()
        {
            var builder = container.Resolve<IBundleCollectionInitializer>();
            var bundles = container.Resolve<BundleCollection>();
            builder.Initialize(bundles);
        }

        void RunStartUpTasks()
        {
            var startUpTasks = container.ResolveAll<IStartUpTask>();
            foreach (var startUpTask in startUpTasks)
            {
                Trace.Source.TraceInformation("Running start-up task: {0}", startUpTask.GetType().FullName);
                startUpTask.Start();
            }
        }

        CassetteSettings CreateSettings(TinyIoCContainer currentContainer)
        {
            lock (createSettingsLock)
            {
                // Only create settings once, then reuse that instance.
                if (settings != null) return settings;

                settings = CreateSettings();
                ConfigureSettings(settings, currentContainer);
                return settings;
            }
        }

        void ConfigureSettings(CassetteSettings settingsToConfigure, TinyIoCContainer currentContainer)
        {
            var configurations = currentContainer.ResolveAll<IConfiguration<CassetteSettings>>();
            foreach (var configuration in configurations)
            {
                configuration.Configure(settingsToConfigure);
            }
        }

        protected virtual CassetteSettings CreateSettings()
        {
            return new CassetteSettings
            {
                Version = GetHostVersion(),
                PrecompiledManifestFile = new NonExistentFile("")
            };
        }

        protected virtual string GetHostVersion()
        {
            return assemblyScanner.HashAssemblies();
        }

        /// <summary>
        /// A separate <see cref="IFileSearch"/> is stored in the container for each type of bundle.
        /// This method returns a name that identifies the FileSearch for a particular bundle type.
        /// </summary>
        internal static string FileSearchComponentName(Type bundleType)
        {
            return bundleType.Name + ".FileSearch";
        }

        public virtual void Dispose()
        {
            container.Dispose();
        }
    }
}