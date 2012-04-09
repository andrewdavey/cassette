using System.IO;
using Cassette.Configuration;
using Moq;
using Xunit;
using Cassette.IO;
using System;
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
            var settings = new CassetteSettings("")
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory)
            };
            bundles = new BundleCollection(settings, t => null, Mock.Of<IBundleFactoryProvider>());
            bundleDefinition = new Mock<IBundleDefinition>();

            rebuilder = new FileSystemWatchingBundleRebuilder(settings, bundles, new[] { bundleDefinition.Object });
        }

        [Fact]
        public void WhenNewFileCreated_ThenBundleDefinitionIsUsedToRebuildBundleCollection()
        {
            rebuilder.Run();

            File.WriteAllText(Path.Combine(tempDirectory, "test.js"), "");

            bundleDefinition.Verify(d => d.AddBundles(bundles), Times.Once());
        }

        [Fact]
        public void WhenFileDeleted_ThenBundleDefinitionIsUsedToRebuildBundleCollection()
        {
            var filename = Path.Combine(tempDirectory, "test.js");
            File.WriteAllText(filename, "");

            rebuilder.Run();

            File.Delete(filename);

            bundleDefinition.Verify(d => d.AddBundles(bundles), Times.Once());
        }

        public void Dispose()
        {
            rebuilder.Dispose();
            tempDirectory.Dispose();
        }
    }
}