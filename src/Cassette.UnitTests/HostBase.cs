using System;
using System.Collections.Generic;
using System.Reflection;
using Moq;
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
            return ConfigurationTypes;
        }

        protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider RequestLifetimeProvider
        {
            get { return Mock.Of<TinyIoCContainer.ITinyIoCObjectLifetimeProvider>(); }
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            yield return typeof(HostBase).Assembly;
        }

        protected override Configuration.CassetteSettings CreateSettings()
        {
            var settings = base.CreateSettings();
            settings.CacheDirectory = new FakeFileSystem();
            return settings;
        }

        protected override void RegisterContainerItems()
        {
            base.RegisterContainerItems();
            Container.Register<IUrlModifier>(new VirtualDirectoryPrepender("/"));
            Container.Register<IBundleCollectionInitializer, BundleCollectionInitializer>();
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

}