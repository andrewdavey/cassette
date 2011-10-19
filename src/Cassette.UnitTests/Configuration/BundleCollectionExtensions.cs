using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleCollectionExtensions_Tests
    {
        [Fact]
        public void GivenTwoBundleDirectories_WhenAddForEachSubDirectory_ThenTwoBundlesAreAddedToTheCollection()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle-a"));
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle-b"));

                var application = new ConfigurableCassetteApplication
                {
                    Settings =
                    {
                        SourceDirectory = new FileSystemDirectory(tempDirectory)
                    }
                };

                var factory = new Mock<IBundleFactory<Bundle>>();
                var bundles = new Queue<Bundle>(new[] { new Bundle("~/test/bundle-a"), new Bundle("~/test/bundle-b") });
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(bundles.Dequeue);
                application.BundleFactories[typeof(Bundle)] = factory.Object;

                var collection = new BundleCollection(application);
                collection.AddForEachSubDirectory<Bundle>("~/test");

                var result = collection.ToArray();
                result[0].Path.ShouldEqual("~/test/bundle-a");
                result[1].Path.ShouldEqual("~/test/bundle-b");
            }
        }

        [Fact]
        public void GivenBundleDirectoryWithDescriptorFile_WhenAddForEachSubDirectory_ThenDescriptorPassedToBundleFactory()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle"));
                File.WriteAllText(Path.Combine(tempDirectory, "test", "bundle", "bundle.txt"), "");
                var application = new ConfigurableCassetteApplication
                {
                    Settings =
                    {
                        SourceDirectory = new FileSystemDirectory(tempDirectory)
                    }
                };
                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new Bundle("~/test/bundle"));
                application.BundleFactories[typeof(Bundle)] = factory.Object;

                var collection = new BundleCollection(application);
                collection.AddForEachSubDirectory<Bundle>("~/test");
                
                factory.Verify(f => f.CreateBundle("~/test/bundle", It.Is<BundleDescriptor>(b => b != null)));
            }
        }
    }
}