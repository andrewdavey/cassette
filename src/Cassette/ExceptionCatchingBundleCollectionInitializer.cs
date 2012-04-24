using System;
namespace Cassette
{
    class ExceptionCatchingBundleCollectionInitializer : IBundleCollectionInitializer
    {
        readonly IBundleCollectionInitializer initializerImplementation;

        public ExceptionCatchingBundleCollectionInitializer(IBundleCollectionInitializer initializerImplementation)
        {
            this.initializerImplementation = initializerImplementation;
        }

        public void Initialize(BundleCollection bundleCollection)
        {
            try
            {
                initializerImplementation.Initialize(bundleCollection);
                bundleCollection.InitializationException = null;
            }
            catch (Exception exception)
            {
                bundleCollection.InitializationException = exception;
            }
        }
    }
}