using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;
using TinyIoC;

namespace Cassette
{
    /// <summary>
    /// A host initializes Cassette for an application.
    /// </summary>
    public abstract class HostBase
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

            Container.Register(typeof(IUrlGenerator), typeof(UrlGenerator));
            Container.Register(typeof(IReferenceBuilder), typeof(ReferenceBuilder));
            Container.Register(typeof(IPlaceholderTracker), typeof(PlaceholderTracker));
            Container.Register(typeof(IJavaScriptMinifier), typeof(MicrosoftJavaScriptMinifier));
            Container.Register(typeof(IStylesheetMinifier), typeof(MicrosoftStylesheetMinifier));

            Container.Register(typeof(ICassetteManifestCache), (c, p) =>
            {
                var cacheFile = c.Resolve<CassetteSettings>().CacheDirectory.GetFile("cassette.xml");
                return new CassetteManifestCache(cacheFile);
            });
            Container.Register(typeof(PrecompiledBundleCollectionBuilder), (c, p) =>
            {
                var file = Container.Resolve<CassetteSettings>().PrecompiledManifestFile;
                return new PrecompiledBundleCollectionBuilder(file, c.Resolve<IUrlModifier>());
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

        protected virtual IEnumerable<Type> GetStartUpTaskTypes()
        {
            return AppDomainAssemblyTypeScanner.TypesOf<IStartUpTask>();
        }

        void CreateBundles()
        {
            var builder = Container.Resolve<BundleCollectionBuilder>();
            var bundles = Container.Resolve<BundleCollection>();
            builder.BuildBundleCollection(bundles);
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
                return new CassetteSettings();
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