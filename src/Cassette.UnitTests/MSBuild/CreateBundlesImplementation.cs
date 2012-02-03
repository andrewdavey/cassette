using System;
using Cassette.Configuration;
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
        public void ItAssignsSettingsUrlGenerator()
        {
            SetupConfig();
            task.Execute();
            configuration.SettingsPassedToConfigure.UrlGenerator.ShouldNotBeNull();
        }

        [Fact]
        public void GivenConfigurationAssignsUrlGenerator_ThenItShouldNotOverwriteUrlGenerator()
        {
            var customUrlGenerator = Mock.Of<IUrlGenerator>();
            SetupConfig(c => c.CustomUrlGenerator = customUrlGenerator);

            task.Execute();

            configuration.SettingsPassedToConfigure.UrlGenerator.ShouldBeSameAs(customUrlGenerator);
        }

        void SetupConfig(Action<MockConfiguration> customize = null)
        {
            bundle = new TestableBundle("~");
            configuration = new MockConfiguration(bundle);

            if (customize != null) customize(configuration);

            var configurationFactory = new Mock<ICassetteConfigurationFactory>();
            configurationFactory
                .Setup(f => f.CreateCassetteConfigurations())
                .Returns(() => new[] { configuration });
            
            task = new CreateBundlesImplementation(
                configurationFactory.Object,
                writer.Object
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

            public IUrlGenerator CustomUrlGenerator { get; set; }

            public void Configure(BundleCollection bundles, CassetteSettings settings)
            {
                this.settings = settings;
                ConfigureWasCalled = true;
                if (CustomUrlGenerator != null)
                {
                    settings.UrlGenerator = CustomUrlGenerator;
                }
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