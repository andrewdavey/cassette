using Cassette.TinyIoC;

namespace Cassette.RequireJS
{
    [ConfigurationOrder(20)] // After core Cassette configuration, but probably before application configuration.
    public class ContainerConfiguration : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            RegisterAmdConfiguration(container);
            RegisterConfigurationScriptBuilder(container);
            RegisterRequireJsConfigUrlProvider(container);
        }

        void RegisterAmdConfiguration(TinyIoCContainer container)
        {
            container
                .Register<AmdConfiguration>()
                .AsSingleton();

            container
                .Register<IAmdModuleCollection>((c, n) => c.Resolve<AmdConfiguration>());
        }

        void RegisterConfigurationScriptBuilder(TinyIoCContainer container)
        {
            container.Register<IConfigurationScriptBuilder>((c, n) =>
            {
                var settings = c.Resolve<CassetteSettings>();
                return new ConfigurationScriptBuilder(
                    c.Resolve<IUrlGenerator>(),
                    c.Resolve<IJsonSerializer>(),
                    settings.IsDebuggingEnabled
                );
            });
        }

        void RegisterRequireJsConfigUrlProvider(TinyIoCContainer container)
        {
            container.Register<IRequireJsConfigUrlProvider>((c, n) => new RequireJsConfigUrlProvider(
                c.Resolve<BundleCollection>(),
                c.Resolve<IAmdModuleCollection>(),
                c.Resolve<IConfigurationScriptBuilder>(),
                c.Resolve<CassetteSettings>().CacheDirectory,
                c.Resolve<IUrlGenerator>()
            ));
        }
    }
}