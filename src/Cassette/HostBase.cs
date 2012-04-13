using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
        Assembly[] allAssemblies;
        Type[] allTypes;
        Type[] configurationTypes;

        public void Initialize()
        {
            allAssemblies = LoadAssemblies().ToArray();
            allTypes = LoadAllTypes(allAssemblies).ToArray();
            configurationTypes = GetConfigurationTypes(allTypes).ToArray();

            container = new TinyIoCContainer();
            RegisterContainerItems();
            RunStartUpTasks();
            CreateBundles();
        }

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
                typeof(ScriptContainerConfiguration),
                typeof(HtmlTemplateContainerConfiguration),
                typeof(StylesheetContainerConfiguration)
            };

            return publicTypes.Concat(internalTypes);
        }

        protected TinyIoCContainer Container
        {
            get { return container; }
        }

        IEnumerable<Type> LoadAllTypes(IEnumerable<Assembly> assemblies)
        {
            return
                from assembly in assemblies
                where !IgnoredAssemblies.Any(ignore => ignore(assembly))
                from type in assembly.GetExportedTypes()
                where !type.IsAbstract
                select type;
        }

        static readonly List<Func<Assembly, bool>> IgnoredAssemblies = new List<Func<Assembly, bool>>
        {
            assembly => assembly.FullName.StartsWith("Microsoft.", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("mscorlib,", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("System.", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("System,", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("IronPython", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("IronRuby", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("CR_ExtUnitTest", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("CR_VSTest", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("DevExpress.CodeRush", StringComparison.InvariantCulture)
        };

        public virtual void Dispose()
        {
            Container.Dispose();
        }

        protected abstract TinyIoCContainer.ITinyIoCObjectLifetimeProvider RequestLifetimeProvider { get; }

        protected abstract IEnumerable<Assembly> LoadAssemblies();

        protected virtual void RegisterContainerItems()
        {
            // There is only ever one BundleCollection for the lifetime of the application.
            Container.Register(typeof(BundleCollection)).AsSingleton();
            Container.Register(typeof(IBundleCollectionInitializer), BundleCollectionInitializerType);

            Container.Register(typeof(IUrlGenerator), typeof(UrlGenerator));
            Container.Register(typeof(IJavaScriptMinifier), typeof(MicrosoftJavaScriptMinifier));
            Container.Register(typeof(IStylesheetMinifier), typeof(MicrosoftStylesheetMinifier));

            if (RequestLifetimeProvider != null)
            {
                Container.Register(typeof(IReferenceBuilder), typeof(ReferenceBuilder)).AsPerRequestSingleton(RequestLifetimeProvider);
                Container.Register(typeof(PlaceholderTracker)).AsPerRequestSingleton(RequestLifetimeProvider);
                Container.Register(typeof(IPlaceholderTracker), (c, p) => GetPlaceholderTracker(c));
            }

            Container.Register(typeof(ICassetteManifestCache), (c, p) =>
            {
                var cacheFile = c.Resolve<CassetteSettings>().CacheDirectory.GetFile("cassette.xml");
                return new CassetteManifestCache(cacheFile);
            });
            Container.Register(typeof(PrecompiledBundleCollectionInitializer), (c, p) =>
            {
                var file = Container.Resolve<CassetteSettings>().PrecompiledManifestFile;
                return new PrecompiledBundleCollectionInitializer(file, c.Resolve<IUrlModifier>());
            });

            Container.RegisterMultiple(typeof(IStartUpTask), GetStartUpTaskTypes());
            
            Container.Register(typeof(CassetteSettings), CreateSettings());
            Container.Register(
                typeof(IBundleFactoryProvider),
                (c, p) => new BundleFactoryProvider(
                    bundleType =>
                    {
                        var factoryType = typeof(IBundleFactory<>).MakeGenericType(bundleType);
                        return (IBundleFactory<Bundle>)c.Resolve(factoryType);
                    })
            );
            Container.Register(
                typeof(IFileSearchProvider),
                (c, p) => new FileSearchProvider(
                    bundleType => c.Resolve<IFileSearch>(FileSearchComponentName(bundleType))
                )
            );

            ExecuteContainerConfigurations();

            RegisterConfigurationTypes();
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
            var containerConfigurations =
                from type in configurationTypes
                where typeof(IConfiguration<TinyIoCContainer>).IsAssignableFrom(type)
                orderby ConfigurationOrderAttribute.GetOrder(type)
                select CreateContainerConfiguration(type);
            return containerConfigurations;
        }

        IConfiguration<TinyIoCContainer> CreateContainerConfiguration(Type type)
        {
            var ctor = type.GetConstructors().First();
            var isDefaultConstructor = ctor.GetParameters().Length == 0;
            if (isDefaultConstructor)
            {
                return (IConfiguration<TinyIoCContainer>)Activator.CreateInstance(type);
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
                Container.RegisterMultiple(configs.Key, configs);
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

        IPlaceholderTracker GetPlaceholderTracker(TinyIoCContainer container)
        {
            if (container.Resolve<CassetteSettings>().IsHtmlRewritingEnabled)
            {
                return container.Resolve<PlaceholderTracker>();
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

        void CreateBundles()
        {
            var builder = Container.Resolve<IBundleCollectionInitializer>();
            var bundles = Container.Resolve<BundleCollection>();
            builder.Initialize(bundles);
        }

        void RunStartUpTasks()
        {
            var startUpTasks = Container.ResolveAll<IStartUpTask>();
            foreach (var startUpTask in startUpTasks)
            {
                Trace.Source.TraceInformation("Running start-up task: {0}", startUpTask.GetType().FullName);
                startUpTask.Start();
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
            return HashAppDomainAssemblies();
        }

        string HashAppDomainAssemblies()
        {
            using (var allHashes = new MemoryStream())
            using (var sha1 = SHA1.Create())
            {
                var filenames = allAssemblies
                    .Where(IsNotDynamic)
                    .Select(assembly => assembly.Location)
                    .OrderBy(filename => filename);

                foreach (var filename in filenames)
                {
                    using (var file = File.OpenRead(filename))
                    {
                        var hash = sha1.ComputeHash(file);
                        allHashes.Write(hash, 0, hash.Length);
                    }
                }
                allHashes.Position = 0;
                return Convert.ToBase64String(sha1.ComputeHash(allHashes));
            }
        }

        bool IsNotDynamic(Assembly assembly)
        {
#if NET35
            return !(assembly.ManifestModule is ModuleBuilder);
#else
            return !assembly.IsDynamic;
#endif
        }

        /// <summary>
        /// A separate <see cref="IFileSearch"/> is stored in the container for each type of bundle.
        /// This method returns a name that identifies the FileSearch for a particular bundle type.
        /// </summary>
        internal static string FileSearchComponentName(Type bundleType)
        {
            return bundleType.Name + ".FileSearch";
        }
    }
}