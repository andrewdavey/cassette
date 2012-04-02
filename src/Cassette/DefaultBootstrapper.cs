using System;
using System.Collections.Generic;
using Cassette.Configuration;
using TinyIoC;

namespace Cassette
{
    public abstract class DefaultBootstrapperBase : BootstrapperBase<TinyIoCContainer>
    {
        protected override TinyIoCContainer CreateContainer()
        {
            return new TinyIoCContainer();
        }

        protected override void RegisterTypesAsSingletons(IEnumerable<TypeRegistration> typeRegistrations, TinyIoCContainer container)
        {
            foreach (var typeRegistration in typeRegistrations)
            {
                container
                    .Register(typeRegistration.RegistrationType, typeRegistration.ImplementationType, typeRegistration.Name)
                    .AsSingleton();
            }
        }

        protected override void RegisterCollectionTypes(IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations, TinyIoCContainer container)
        {
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                container.RegisterMultiple(collectionTypeRegistration.RegistrationType, collectionTypeRegistration.ImplementationTypes);
            }
        }

        protected override void RegisterInstances(IEnumerable<InstanceRegistration> instanceRegistrations, TinyIoCContainer container)
        {
            foreach (var instanceRegistration in instanceRegistrations)
            {
                if (string.IsNullOrEmpty(instanceRegistration.Name))
                {
                    container.Register(instanceRegistration.RegistrationType, instanceRegistration.Instance);
                }
                else
                {
                    container.Register(instanceRegistration.RegistrationType, instanceRegistration.Instance, instanceRegistration.Name);
                }
            }
        }

        protected override BundleCollection CreateBundleCollection(TinyIoCContainer container)
        {
            return container.Resolve<BundleCollection>();
        }

        protected override IEnumerable<IBundleDefinition> CreateBundleDefinitions(TinyIoCContainer container)
        {
            return container.ResolveAll<IBundleDefinition>();
        }

        protected override IEnumerable<IStartUpTask> CreateStartUpTasks(TinyIoCContainer container)
        {
            return container.ResolveAll<IStartUpTask>();
        }

        protected override IFileSearch GetFileSearch(string name, TinyIoCContainer container)
        {
            return container.Resolve<IFileSearch>(name);
        }

        protected override IBundleFactory<Bundle> GetBundleFactory(Type bundleType, TinyIoCContainer container)
        {
            return (IBundleFactory<Bundle>)container.Resolve(typeof(IBundleFactory<>).MakeGenericType(bundleType));
        }
    }
}