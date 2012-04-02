using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette
{
    public abstract class BootstrapperBase<TContainer> : IBootstrapper
    {
        TContainer container;

        readonly List<InstanceRegistration> additionalInstanceRegistrations = new List<InstanceRegistration>();

        public void Initialize()
        {
            container = CreateContainer();
            RegisterContainerItems();
            CreateBundles();
            RunStartUpTasks();
        }

        protected abstract TContainer CreateContainer();

        void RegisterContainerItems()
        {
            var contributors = BootstrapperContributors.ToArray();

            var typeRegistrations = TypeRegistrations
                .Concat(contributors.SelectMany(c => c.TypeRegistrations));
            RegisterTypesAsSingletons(typeRegistrations, container);

            var collectionTypeRegistrations = CollectionTypeRegistrations
                .Concat(contributors.SelectMany(c => c.CollectionTypeRegistrations));
            RegisterCollectionTypes(collectionTypeRegistrations, container);

            var instanceRegistrations = InstanceRegistrationTypes
                .Concat(contributors.SelectMany(c => c.InstanceRegistrations))
                .Concat(additionalInstanceRegistrations);
            RegisterInstances(instanceRegistrations, container);
        }

        void CreateBundles()
        {
            var bundles = CreateBundleCollection(container);
            var bundleDefinitions = CreateBundleDefinitions(container);
            bundles.AddRange(bundleDefinitions);
        }

        protected abstract BundleCollection CreateBundleCollection(TContainer container);

        protected abstract IEnumerable<IBundleDefinition> CreateBundleDefinitions(TContainer container);

        void RunStartUpTasks()
        {
            var startUpTasks = CreateStartUpTasks(container);
            foreach (var startUpTask in startUpTasks)
            {
                startUpTask.Run();
            }
        }

        protected abstract void RegisterTypesAsSingletons(IEnumerable<TypeRegistration> typeRegistrations, TContainer container);

        protected abstract void RegisterCollectionTypes(IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations, TContainer container);

        protected abstract void RegisterInstances(IEnumerable<InstanceRegistration> instanceRegistrations, TContainer container);

        protected abstract IEnumerable<IStartUpTask> CreateStartUpTasks(TContainer container);

        protected abstract IFileSearch GetFileSearch(string name, TContainer container);

        protected abstract IBundleFactory<Bundle> GetBundleFactory(Type bundleType, TContainer container);

        protected virtual IEnumerable<IBootstrapperContributor> BootstrapperContributors
        {
            get
            {
                return AppDomainAssemblyTypeScanner.TypesOf<IBootstrapperContributor>()
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .Select(type => (IBootstrapperContributor)Activator.CreateInstance(type));
            }
        }

        protected virtual IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                return new[]
                {
                    new TypeRegistration(typeof(IUrlModifier), UrlModifier),
                    new TypeRegistration(typeof(IUrlGenerator), UrlGenerator),
                    new TypeRegistration(typeof(IJavaScriptMinifier), JavaScriptMinifier),
                    new TypeRegistration(typeof(IStylesheetMinifier), CssMinifier),
                    new TypeRegistration(typeof(BundleCollection), typeof(BundleCollection)),
                    new TypeRegistration(typeof(BundleCollectionBuilder), typeof(BundleCollectionBuilder)),
                    new TypeRegistration(typeof(DebugModeBundleCollectionBuilder), typeof(DebugModeBundleCollectionBuilder)),
                    new TypeRegistration(typeof(ProductionModeBundleCollectionBuilder), typeof(ProductionModeBundleCollectionBuilder)),
                    new TypeRegistration(typeof(PrecompiledBundleCollectionBuilder), typeof(PrecompiledBundleCollectionBuilder)),
                    new TypeRegistration(typeof(ICassetteManifestCache), typeof(CassetteManifestCache)) 
                };
            }
        }

        protected virtual IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                return new[]
                {
                    new CollectionTypeRegistration(typeof(IStartUpTask), StartUpTasks), 
                    new CollectionTypeRegistration(typeof(IBundleDefinition), BundleDefinitions)
                };
            }
        }

        protected virtual IEnumerable<InstanceRegistration> InstanceRegistrationTypes
        {
            get
            {
                return new[]
                {
                    new InstanceRegistration(typeof(CassetteSettings), Settings),
                    new InstanceRegistration(typeof(IBundleFactoryProvider), BundleFactoryProvider), 
                    new InstanceRegistration(
                        typeof(Func<Type, IFileSearch>),
                        new Func<Type, IFileSearch>(bundleType => GetFileSearch(FileSearchComponentName(bundleType), container))
                    )
                };
            }
        }

        protected virtual Type UrlModifier
        {
            get { return typeof(VirtualDirectoryPrepender); }
        }

        protected virtual Type UrlGenerator
        {
            get { return typeof(UrlGenerator); }
        }

        protected virtual Type JavaScriptMinifier
        {
            get { return typeof(MicrosoftJavaScriptMinifier); }
        }

        protected virtual Type CssMinifier
        {
            get { return typeof(MicrosoftStylesheetMinifier); }
        }

        protected virtual IEnumerable<Type> StartUpTasks
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IStartUpTask>(); }
        }

        protected virtual IEnumerable<Type> BundleDefinitions
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IBundleDefinition>(); }
        }

        protected virtual IBundleFactoryProvider BundleFactoryProvider
        {
            get { return new BundleFactoryProvider(t => GetBundleFactory(t, container)); }
        }

        protected virtual CassetteSettings Settings
        {
            get
            {
                return new CassetteSettings(GetVersion());
            }
        }

        string GetVersion()
        {
            throw new NotImplementedException();
        }

        protected void SetDefaultFileSearch<T>(IFileSearch fileSearch)
        {
            // This will override the existing named file search object in the container.
            additionalInstanceRegistrations.Add(
                new InstanceRegistration(typeof(IFileSearch), fileSearch, FileSearchComponentName(typeof(T)))
            );
        }

        // TODO: SetDefaultBundlePipeline<T>?

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