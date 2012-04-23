using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette
{
    public class HostBase_Tests
    {
        [Fact]
        public void InitializeCallsContainerConfigurations()
        {
            var host = new Host
            {
                ConfigurationTypes = new[]
                {
                    typeof(ContainerConfiguration)
                }
            };
            host.Initialize();
            host.Container.Resolve<object>("ContainerConfigurationWasCalled").ShouldNotBeNull();
        }

        [Fact]
        public void InitializeCallsContainerConfigurationsInOrder()
        {
            var host = new Host
            {
                ConfigurationTypes = new[]
                {
                    // Deliberate wrong order here:
                    typeof(SecondContainerConfiguration),
                    typeof(FirstContainerConfiguration)
                }
            };
            host.Initialize();

            host.Container.Resolve<string>("Order").ShouldEqual("FirstSecond");
        }

        [Fact]
        public void InitializeCallsSettingsConfigurations()
        {
            var host = new Host
            {
                ConfigurationTypes = new[]
                {
                    typeof(SettingsConfiguration)
                }
            };
            host.Initialize();

            var settings = host.Container.Resolve<CassetteSettings>();
            settings.Version.ShouldEqual("SettingsConfigurationWasCalled");
        }

        [Fact]
        public void CassetteSettingsIsSingleton()
        {
            var host = new Host();
            host.Initialize();
            var settings1 = host.Container.Resolve<CassetteSettings>();
            var settings2 = host.Container.Resolve<CassetteSettings>();
            settings1.ShouldBeSameAs(settings2);
        }
    }

    class Host : HostBase
    {
        public IEnumerable<Type> ConfigurationTypes { get; set; }

        public new TinyIoCContainer Container
        {
            get { return base.Container; }
        }

        protected override IEnumerable<Type> GetConfigurationTypes(IEnumerable<Type> typesToSearch)
        {
            return ConfigurationTypes ?? Enumerable.Empty<Type>();
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return false; }
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            yield return typeof(HostBase).Assembly;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.Register<IUrlModifier>(new VirtualDirectoryPrepender("/"));
            Container.Register<IBundleCollectionInitializer, BundleCollectionInitializer>();
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new TestSettingsConfiguration();
        }

        class TestSettingsConfiguration : IConfiguration<CassetteSettings>
        {
            public void Configure(CassetteSettings settings)
            {
                settings.CacheDirectory = new FakeFileSystem();
            }
        }
    }

    class ContainerConfiguration : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register(new object(), "ContainerConfigurationWasCalled");
        }
    }

    [ConfigurationOrder(1)]
    class FirstContainerConfiguration : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register("First", "Order");
        }
    }

    [ConfigurationOrder(2)]
    class SecondContainerConfiguration : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            var first = container.Resolve<string>("Order");
            container.Register(first + "Second", "Order");
        }
    }

    class SettingsConfiguration : IConfiguration<CassetteSettings>
    {
        public void Configure(CassetteSettings settings)
        {
            settings.Version = "SettingsConfigurationWasCalled";
        }
    }
}