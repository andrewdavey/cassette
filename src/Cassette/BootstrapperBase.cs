using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
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
            var contributors = BootstrapperContributors.ToArray();

            container = CreateContainer();

            var typeRegistrations = TypeRegistrations.Concat(contributors.SelectMany(c => c.TypeRegistrations));
            RegisterTypesAsSingletons(typeRegistrations, container);

            var collectionTypeRegistrations = CollectionTypeRegistrations.Concat(contributors.SelectMany(c => c.CollectionTypeRegistrations));
            RegisterCollectionTypes(collectionTypeRegistrations, container);

            var instanceRegistrations = InstanceRegistrationTypes.Concat(contributors.SelectMany(c => c.InstanceRegistrations)).Concat(additionalInstanceRegistrations);
            RegisterInstances(instanceRegistrations, container);
        }

        public ICassetteApplication GetApplication()
        {
            return GetApplication(container);
        }

        protected abstract TContainer CreateContainer();

        protected abstract void RegisterTypesAsSingletons(IEnumerable<TypeRegistration> typeRegistrations, TContainer container);

        protected abstract void RegisterCollectionTypes(IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations, TContainer container);

        protected abstract void RegisterInstances(IEnumerable<InstanceRegistration> instanceRegistrations, TContainer container);

        protected abstract ICassetteApplication GetApplication(TContainer container);

        protected abstract IFileSearch GetFileSearch(TContainer container, string name);

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
                    new TypeRegistration(typeof(BundleCollectionX), typeof(BundleCollectionX))
                };
            }
        }

        protected virtual IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                return new[]
                {
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
                    new InstanceRegistration(
                        typeof(Func<Type, IFileSearch>),
                        new Func<Type, IFileSearch>(bundleType => GetFileSearch(container, FileSearchComponentName(bundleType)))
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

        protected virtual IEnumerable<Type> BundleDefinitions
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IBundleDefinition>(); }
        }

        protected virtual CassetteSettings Settings
        {
            get { return new CassetteSettings(""); }
        }

        protected void SetDefaultFileSearch<T>(IFileSearch fileSearch)
        {
            additionalInstanceRegistrations.Add(
                new InstanceRegistration(typeof(IFileSearch), fileSearch, FileSearchComponentName(typeof(T)))
                );
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