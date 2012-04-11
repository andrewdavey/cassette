using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
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
        protected TinyIoCContainer Container;
        Type[] types;

        public void Initialize()
        {
            LoadAllTypes();
            Container = new TinyIoCContainer();
            RegisterContainerItems();
            RunStartUpTasks();
            CreateBundles();
        }

        void LoadAllTypes()
        {
            var assemblies = LoadAssemblies();
            types = (from assembly in assemblies
                     where !IgnoredAssemblies.Any(ignore => ignore(assembly))
                     from type in assembly.GetExportedTypes()
                     where !type.IsAbstract
                     select type).ToArray();
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
            Container.RegisterMultiple(typeof(IBundleDefinition), GetImplementationTypes(typeof(IBundleDefinition)));

            Container.Register(typeof(CassetteSettings), Settings);
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

            new ScriptBundleContainerBuilder(GetImplementationTypes).Build(Container);
            new StylesheetBundleContainerBuilder(GetImplementationTypes).Build(Container);
            new HtmlTemplateBundleContainerBuilder(GetImplementationTypes).Build(Container);

            foreach (var containerBuilder in ContainerBuilders)
            {
                containerBuilder.Build(Container);
            }

            foreach (var serviceRegistry in ServiceRegistries)
            {
                foreach (var registration in serviceRegistry.TypeRegistrations)
                {
                    Container.Register(registration.RegistrationType, registration.ImplementationType, registration.Name);
                }
                foreach (var registration in serviceRegistry.CollectionTypeRegistrations)
                {
                    Container.RegisterMultiple(registration.RegistrationType, registration.ImplementationTypes);
                }
                foreach (var registration in serviceRegistry.InstanceRegistrations)
                {
                    Container.Register(registration.RegistrationType, registration.Instance, registration.Name);
                }
            }
        }

        IEnumerable<Type> GetImplementationTypes(Type baseType)
        {
            return types.Where(baseType.IsAssignableFrom); 
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

        IEnumerable<IContainerBuilder> ContainerBuilders
        {
            get
            {
                return GetImplementationTypes(typeof(IContainerBuilder))
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .Select(type => (IContainerBuilder)Activator.CreateInstance(type));
            }
        }

        protected virtual IEnumerable<IServiceRegistry> ServiceRegistries
        {
            get
            {
                return GetImplementationTypes(typeof(IServiceRegistry))
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .Select(type => (IServiceRegistry)Activator.CreateInstance(type));
            }
        }

        protected virtual CassetteSettings Settings
        {
            get
            {
                return new CassetteSettings
                {
                    Version = HashAppDomainAssemblies()
                };
            }
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
                var filenames = AppDomain.CurrentDomain
                    .GetAssemblies()
#if NET35
                    .Where(assembly => !(assembly.ManifestModule is ModuleBuilder))
#else
                    .Where(assembly => !assembly.IsDynamic)
#endif
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