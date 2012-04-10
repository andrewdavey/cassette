using System;
using System.IO;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Manifests;
using Moq;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
    public class CreateBundlesImplementation_Execute_Tests : IDisposable
    {
        MockConfiguration configuration;
        readonly TempDirectory path;
        readonly Mock<ICassetteManifestWriter> writer;
        Bundle bundle;
        CreateBundlesImplementation task;
        IDirectory sourceDirectory;

        public CreateBundlesImplementation_Execute_Tests()
        {
            path = new TempDirectory();
            writer = new Mock<ICassetteManifestWriter>();
        }

        [Fact]
        public void ItCallsConfigurationConfigure()
        {
            SetupConfig();
            task.Execute();
            configuration.ConfigureWasCalled.ShouldBeTrue();
        }

        [Fact]
        public void ItProcessesBundles()
        {
            SetupConfig();
            task.Execute();
            bundle.IsProcessed.ShouldBeTrue();
        }

        [Fact]
        public void ItWritesManifestWithWriter()
        {
            SetupConfig();
            task.Execute();
            writer.Verify(w => w.Write(It.IsAny<CassetteManifest>()));
        }

        [Fact]
        public void ItAssignsSettingsSourceDirectory()
        {
            SetupConfig();
            task.Execute();
            configuration.SettingsPassedToConfigure.SourceDirectory.ShouldBeSameAs(sourceDirectory);
        }

        void SetupConfig(Action<MockConfiguration> customize = null)
        {
            bundle = new TestableBundle("~");
            configuration = new MockConfiguration(bundle);
            sourceDirectory = Mock.Of<IDirectory>();

            if (customize != null) customize(configuration);

            var configurationFactory = new Mock<ICassetteConfigurationFactory>();
            configurationFactory
                .Setup(f => f.CreateCassetteConfigurations())
                .Returns(() => new[] { configuration });

            var settings = new CassetteSettings();
            var bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            task = new CreateBundlesImplementation(
                Path.Combine(path, "cassette.xml"),
                bundles,
                settings
            );
        }

        public void Dispose()
        {
            path.Dispose();
        }

        class MockConfiguration : ICassetteConfiguration
        {
            readonly Bundle bundle;
            CassetteSettings settings;

            public MockConfiguration(Bundle bundle)
            {
                this.bundle = bundle;
            }

            public void Configure(BundleCollection bundles, CassetteSettings settings)
            {
                this.settings = settings;
                ConfigureWasCalled = true;
                bundles.Add(bundle);
            }

            public bool ConfigureWasCalled { get; set; }

            public CassetteSettings SettingsPassedToConfigure
            {
                get { return settings; }
            }
        }
    }
}