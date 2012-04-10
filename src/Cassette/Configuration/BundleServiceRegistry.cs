using System;
using System.Collections.Generic;
using Cassette.BundleProcessing;

namespace Cassette.Configuration
{
    public abstract class BundleServiceRegistry<T> : ServiceRegistry
        where T : Bundle
    {
        public override IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                yield return new CollectionTypeRegistration(typeof(IFileSearchModifier<T>), FileSearchModifiers);
                yield return new CollectionTypeRegistration(typeof(IBundlePipelineModifier<T>), BundlePipelineModifiers);
            }
        }

        public override IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                yield return new TypeRegistration(typeof(IBundlePipeline<T>), BundlePipeline);
                yield return new TypeRegistration(typeof(IBundleFactory<T>), BundleFactory);
                yield return new TypeRegistration(typeof(IFileSearch), typeof(FileSearch<T>), HostBase.FileSearchComponentName(typeof(T)));
            }
        }

        IEnumerable<Type> FileSearchModifiers
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IFileSearchModifier<T>>(); }
        }

        IEnumerable<Type> BundlePipelineModifiers
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IBundlePipelineModifier<T>>(); }
        }

        protected abstract Type BundlePipeline { get; }

        protected abstract Type BundleFactory { get; }
    }
}