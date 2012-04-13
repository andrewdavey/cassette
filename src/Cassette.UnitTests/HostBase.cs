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
        public void _()
        {
            var host = new Host();
            host.Initialize();
            host.Container.Resolve<object>("ContainerConfigurationWasCalled").ShouldNotBeNull();
        }
    }

    class Host : HostBase
    {
        public new TinyIoCContainer Container
        {
            get { return base.Container; }
        }

        protected override IEnumerable<Type> GetConfigurationTypes()
        {
            yield return typeof(ContainerConfiguration);
        }

        protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider RequestLifetimeProvider
        {
            get { return Mock.Of<TinyIoCContainer.ITinyIoCObjectLifetimeProvider>(); }
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            yield return typeof(HostBase).Assembly;
        }

        protected override Configuration.CassetteSettings Settings
        {
            get
            {
                var settings = base.Settings;
                settings.CacheDirectory = new FakeFileSystem();
                return settings;
            }
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
}