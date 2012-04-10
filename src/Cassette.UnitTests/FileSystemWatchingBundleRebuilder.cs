using System;
using System.IO;
using System.Threading;
using Cassette.Configuration;
using Cassette.IO;
using Moq;
using Xunit;

namespace Cassette
{
    public class FileSystemWatchingBundleRebuilder_Tests : IDisposable
    {
        readonly BundleCollection bundles;
        readonly FileSystemWatchingBundleRebuilder rebuilder;
        readonly Mock<IBundleDefinition> bundleDefinition;
        readonly TempDirectory tempDirectory;

        public FileSystemWatchingBundleRebuilder_Tests()
        {
            tempDirectory = new TempDirectory();
            var settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory)
            };
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            bundleDefinition = new Mock<IBundleDefinition>();

            rebuilder = new FileSystemWatchingBundleRebuilder(settings, bundles, new[] { bundleDefinition.Object });
        }

        [Fact]
        public void WhenNewFileCreated_ThenBundleDefinitionIsUsedToRebuildBundleCollection()
        {
            rebuilder.Run();

            File.WriteAllText(Path.Combine(tempDirectory, "test.js"), "");
            Thread.Sleep(200); // Wait for the file system change event to fire.

            bundleDefinition.Verify(d => d.AddBundles(bundles), Times.Once());
        }

        [Fact]
        public void WhenFileDeleted_ThenBundleDefinitionIsUsedToRebuildBundleCollection()
        {
            var filename = Path.Combine(tempDirectory, "test.js");
            File.WriteAllText(filename, "");

            rebuilder.Run();

            File.Delete(filename);
            Thread.Sleep(200); // Wait for the file system change event to fire.

            bundleDefinition.Verify(d => d.AddBundles(bundles), Times.Once());
        }

        public void Dispose()
        {
            rebuilder.Dispose();
            tempDirectory.Dispose();
        }
    }
}