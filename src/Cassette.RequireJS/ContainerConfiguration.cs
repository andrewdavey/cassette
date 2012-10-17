using Cassette.TinyIoC;

namespace Cassette.RequireJS
{
    [ConfigurationOrder(20)] // After core Cassette configuration, but probably before application configuration.
    public class ContainerConfiguration : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            RegisterConfigurationScriptBuilder(container);
        }

        static void RegisterConfigurationScriptBuilder(TinyIoCContainer container)
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
    }
}