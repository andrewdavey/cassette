using System;
using System.IO;
using System.Linq;
using System.Web;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class CassetteApplicationContainerFactory_Test
    {
        readonly CassetteConfigurationSection configurationSection;

        public CassetteApplicationContainerFactory_Test()
        {
            configurationSection = new CassetteConfigurationSection();            
        }

        [Fact]
        public void GivenStylesheetWithExternalReference_WhenCreateContainer_ThenExternalBundleAddedToBundleCollection()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "styles"));
                File.WriteAllText(Path.Combine(path, "styles", "asset.css"), "/* @reference http://example.com */");

                var configuration = new StubConfiguration(bundles => bundles.Add<StylesheetBundle>("styles"));
                var factory = new CassetteApplicationContainerFactory(
                    new DelegateCassetteConfigurationFactory(() => new[] { configuration }),
                    new CassetteConfigurationSection(),
                    path,
                    "/",
                    false,
                    Mock.Of<HttpContextBase>
                );

                var container = factory.CreateContainer();

                container.Application.Bundles.Any(
                    b => b is ExternalStylesheetBundle && b.Path == "http://example.com"
                ).ShouldBeTrue();
            }
        }

        class StubConfiguration : ICassetteConfiguration
        {
            readonly Action<BundleCollection> configure;

            public StubConfiguration(Action<BundleCollection> configure)
            {
                this.configure = configure;
            }

            public void Configure(BundleCollection bundles, CassetteSettings settings)
            {
                configure(bundles);
            }
        }

        [Fact]
        public void GivenCompileTimeManifest_WhenCreateContainer_ThenBundleIsLoadedFromManifest()
        {
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);

                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.FindBundleContainingPath<ScriptBundle>("~/test.js").ShouldNotBeNull();
            }
        }

        [Fact]
        public void GivenCompileTimeManifest_WhenCreateContainer_ThenAdHocExternalReferenceIsConvertedToExternalBundle()
        {
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);

                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.FindBundleContainingPath<ScriptBundle>("http://example.org/").ShouldNotBeNull();
            }
        }

        [Fact]
        public void GivenCompileTimeManifestAndConfigRewriteHtmlIsTrue_WhenCreateContainer_ThenSettingsIsHtmlRewritingEnabledEqualsTrue()
        {
            configurationSection.RewriteHtml = true;
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);
                
                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.Settings.IsHtmlRewritingEnabled.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCompileTimeManifestAndConfigRewriteHtmlIsFalse_WhenCreateContainer_ThenSettingsIsHtmlRewritingEnabledEqualsFalse()
        {
            configurationSection.RewriteHtml = false;
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);

                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.Settings.IsHtmlRewritingEnabled.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenCompileTimeManifestAndConfigAllowRemoteDiagnosticsIsTrue_WhenCreateContainer_ThenSettingsAllowRemoteDiagnosticsEqualsTrue()
        {
            configurationSection.AllowRemoteDiagnostics = true;
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);

                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.Settings.AllowRemoteDiagnostics.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCompileTimeManifestAndConfigAllowRemoteDiagnosticsIsTrue_WhenCreateContainer_ThenSettingsAllowRemoteDiagnosticsEqualsFalse()
        {
            configurationSection.AllowRemoteDiagnostics = false;
            using (var path = new TempDirectory())
            {
                CompileTimeManifestWithBundleExists(path);

                var factory = CreateCassetteApplicationContainerFactory(path);
                var container = factory.CreateContainer();

                container.Application.Settings.AllowRemoteDiagnostics.ShouldBeFalse();
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

            configurationSection.PrecompiledManifest = "App_Data/cassette.xml";
        }

        ScriptBundle StubBundle()
        {
            var bundle = new ScriptBundle("~");
            var asset = new StubAsset(fullPath: "~/test.js");
            asset.References.Add(new AssetReference("http://example.org/", asset, 1, AssetReferenceType.Url));
            bundle.Assets.Add(asset);
            bundle.Process(new CassetteSettings(""));
            bundle.Renderer = new ConstantHtmlRenderer<ScriptBundle>("");
            return bundle;
        }

        CassetteApplicationContainerFactory CreateCassetteApplicationContainerFactory(TempDirectory path)
        {
            return new CassetteApplicationContainerFactory(
                new DelegateCassetteConfigurationFactory(Enumerable.Empty<ICassetteConfiguration>),
                configurationSection,
                path,
                "/",
                false,
                Mock.Of<HttpContextBase>
            );
        }

        IAsset StubAssetX(string filename)
        {
            var asset = new Mock<IAsset>();
            asset
                .Setup(a => a.OpenStream())
                .Returns(Stream.Null);

            asset
                .Setup(a => a.SourceFile.FullPath)
                .Returns(filename);

            asset
                .Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v => v.Visit(asset.Object));

            asset
                .SetupGet(a => a.References)
                .Returns(new[]
                {
                    new AssetReference("http://example.org/", asset.Object, 1, AssetReferenceType.Url)
                });

            return asset.Object;
        }
    }
}