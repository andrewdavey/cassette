using System.IO;
using System.Linq;
using System.Web;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class CassetteApplicationContainerFactory_Tests
    {
        [Fact]
        public void GivenCompileTimeManifestExists_WhenCreateContainer_ThenBundleIsLoadedFromManifest()
        {
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);

                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.FindBundleContainingPath<ScriptBundle>("~/test.js").ShouldNotBeNull();
            }
        }

        void CompileTimeManifestWithBundleExists(string rootDirectory)
        {
            var bundle = StubBundle();
            var bundleManifest = bundle.CreateBundleManifest(true);
            
            var cassetteManifest = new CassetteManifest("", new[] { bundleManifest });

            var manifestFilename = Path.Combine(rootDirectory, "App_Data", "cassette.xml");
            Directory.CreateDirectory(Path.Combine(rootDirectory, "App_Data"));
            using (var outputStream = File.Open(manifestFilename, FileMode.Create, FileAccess.Write))
            {
                var writer = new CassetteManifestWriter(outputStream);
                writer.Write(cassetteManifest);
            }
        }

        ScriptBundle StubBundle()
        {
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(StubAsset("~/test.js"));
            bundle.Process(new CassetteSettings(""));
            return bundle;
        }

        CassetteApplicationContainerFactory CreateCassetteApplicationContainerFactory(TempDirectory path)
        {
            return new CassetteApplicationContainerFactory(
                new DelegateCassetteConfigurationFactory(Enumerable.Empty<ICassetteConfiguration>),
                new CassetteConfigurationSection(),
                path,
                "/",
                false,
                Mock.Of<HttpContextBase>
            );
        }

        IAsset StubAsset(string filename)
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            asset.Setup(a => a.SourceFile.FullPath).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>())).Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            return asset.Object;
        }
    }
}