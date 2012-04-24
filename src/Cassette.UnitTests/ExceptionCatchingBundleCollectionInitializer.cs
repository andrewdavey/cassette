using System;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ExceptionCatchingBundleCollectionInitializer_WhereInitializerImplementationDoesNotThrowException
    {
        readonly ExceptionCatchingBundleCollectionInitializer initializer;
        readonly Mock<IBundleCollectionInitializer> initializerThatThrows;
        readonly BundleCollection bundleCollection;

        public ExceptionCatchingBundleCollectionInitializer_WhereInitializerImplementationDoesNotThrowException()
        {
            bundleCollection = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());

            initializerThatThrows = new Mock<IBundleCollectionInitializer>();

            initializer = new ExceptionCatchingBundleCollectionInitializer(initializerThatThrows.Object);
            initializer.Initialize(bundleCollection);
        }

        [Fact]
        public void ItCallsInitializerImplementation()
        {
            initializerThatThrows.Verify(i => i.Initialize(bundleCollection));
        }
    }

    public class ExceptionCatchingBundleCollectionInitializer_WhereInitializerImplementationThrowsException
    {
        readonly ExceptionCatchingBundleCollectionInitializer initializer;
        readonly Mock<IBundleCollectionInitializer> initializerThatThrows;
        readonly BundleCollection bundleCollection;
        readonly Exception exception;

        public ExceptionCatchingBundleCollectionInitializer_WhereInitializerImplementationThrowsException()
        {
            bundleCollection = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            bundleCollection.Add(new TestableBundle("~")); // Add a bundle so we can check the collection gets cleared.

            initializerThatThrows = new Mock<IBundleCollectionInitializer>();
            exception = new Exception();
            initializerThatThrows
                .Setup(i => i.Initialize(bundleCollection))
                .Throws(exception);

            initializer = new ExceptionCatchingBundleCollectionInitializer(initializerThatThrows.Object);
            initializer.Initialize(bundleCollection);
        }

        [Fact]
        public void ItCallsInitializerImplementation()
        {
            initializerThatThrows.Verify(i => i.Initialize(bundleCollection));
        }

        [Fact]
        public void ItSetsBundleCollectionException()
        {
            bundleCollection.BuildException.ShouldBeSameAs(exception);
        }

        [Fact]
        public void ItClearsBundleCollection()
        {
            bundleCollection.ShouldBeEmpty();
        }
    }
}