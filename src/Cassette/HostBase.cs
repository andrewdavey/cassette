using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;
using TinyIoC;

namespace Cassette
{
    /// <summary>
    /// A host initializes Cassette for an application.
    /// </summary>
    public abstract class HostBase : IDisposable
    {
        protected TinyIoCContainer Container;

        public void Initialize()
        {
            Container = new TinyIoCContainer();
            RegisterContainerItems();
            CreateBundles();
            RunStartUpTasks();
        }

        public virtual void Dispose()
        {
            Container.Dispose();
        }

        protected virtual void RegisterContainerItems()
        {
            // There is only ever one BundleCollection for the lifetime of the application.
            Container.Register(typeof(BundleCollection)).AsSingleton();
            Container.Register(typeof(IBundleCollectionInitializer), BundleCollectionInitializerType);

            Container.Register(typeof(IUrlGenerator), typeof(UrlGenerator));
            Container.Register(typeof(IReferenceBuilder), typeof(ReferenceBuilder));
            Container.Register(typeof(IPlaceholderTracker), (c,p) => GetPlaceholderTracker(c));
            Container.Register(typeof(IJavaScriptMinifier), typeof(MicrosoftJavaScriptMinifier));
            Container.Register(typeof(IStylesheetMinifier), typeof(MicrosoftStylesheetMinifier));

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
            Container.RegisterMultiple(typeof(IBundleDefinition), AppDomainAssemblyTypeScanner.TypesOf<IBundleDefinition>());

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

            new ScriptBundleContainerModule().Load(Container);
            new StylesheetBundleContainerModule().Load(Container);
            new HtmlTemplateBundleContainerModule().Load(Container);

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

        protected virtual Type BundleCollectionInitializerType
        {
            get { return typeof(RuntimeBundleCollectionInitializer); }
        }

        IPlaceholderTracker GetPlaceholderTracker(TinyIoCContainer container)
        {
            if (container.Resolve<CassetteSettings>().IsHtmlRewritingEnabled)
            {
                return new PlaceholderTracker();
            }
            else
            {
                return new NullPlaceholderTracker();
            }
        }

        protected virtual IEnumerable<Type> GetStartUpTaskTypes()
        {
            return AppDomainAssemblyTypeScanner.TypesOf<IStartUpTask>();
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
                startUpTask.Run();
            }
        }

        protected virtual IEnumerable<IServiceRegistry> ServiceRegistries
        {
            get
            {
                return AppDomainAssemblyTypeScanner.TypesOf<IServiceRegistry>()
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
                    .Where(assembly => !assembly.IsDynamic)
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