using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Moq;

namespace Cassette
{
    public abstract class BundleCollectionTestsBase : IDisposable
    {
        protected readonly BundleCollection bundles;
        internal readonly TempDirectory tempDirectory;
        protected readonly Mock<IBundleFactory<TestableBundle>> factory;
        protected readonly Mock<IFileSearch> fileSearch;
        protected readonly CassetteSettings settings;
        protected string[] FilesUsedToCreateBundle;
        protected readonly Mock<IBundleFactoryProvider> bundleFactoryProvider;

        protected BundleCollectionTestsBase()
        {
            tempDirectory = new TempDirectory();
            factory = new Mock<IBundleFactory<TestableBundle>>();
            factory
                .Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Callback<string, IEnumerable<IFile>, BundleDescriptor>((path, files, descriptor) => FilesUsedToCreateBundle = files.Select(f => f.FullPath).ToArray())
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                    (path, files, d) => new TestableBundle(path)
                );

            fileSearch = new Mock<IFileSearch>();
            fileSearch
                .Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns<IDirectory>(d => d.GetFiles("*.*", SearchOption.AllDirectories));

            settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory),
            };

            bundleFactoryProvider = new Mock<IBundleFactoryProvider>();
            bundleFactoryProvider
                .Setup(p => p.GetBundleFactory<TestableBundle>())
                .Returns(factory.Object);

            var fileSearchProvider = new Mock<IFileSearchProvider>();
            fileSearchProvider
                .Setup(p => p.GetFileSearch(It.IsAny<Type>()))
                .Returns(fileSearch.Object);

            bundles = new BundleCollection(settings, fileSearchProvider.Object, bundleFactoryProvider.Object);
        }

        public void Dispose()
        {
            tempDirectory.Dispose();
        }

        protected void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(tempDirectory, path));
        }

        protected IFile StubFile(string path = "")
        {
            var file = new Mock<IFile>();
            file.SetupGet(a => a.FullPath).Returns(path);
            return file.Object;
        }

        protected void SetBundleFactory<T>(Mock<IBundleFactory<T>> factory)
            where T : Bundle
        {
            bundleFactoryProvider.Setup(p => p.GetBundleFactory<T>()).Returns(factory.Object);
        }
    }
}