using Cassette.BundleProcessing;
using TinyIoC;

namespace Cassette.HtmlTemplates
{
    [ConfigurationOrder(20)]
    public class HoganServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register((c, p) => CreateHoganSettings(c));

            container
                .Register(typeof(IBundlePipeline<HtmlTemplateBundle>), typeof(HoganPipeline))
                .AsMultiInstance();
        }

        HoganSettings settings;
        readonly object settingsCreationLock = new object();

        HoganSettings CreateHoganSettings(TinyIoCContainer container)
        {
            lock (settingsCreationLock)
            {
                if (settings != null) return settings;

                settings = new HoganSettings();
                ConfigureHoganSettings(container);
                return settings;
            }
        }

        void ConfigureHoganSettings(TinyIoCContainer container)
        {
            var configurations = container.ResolveAll<IConfiguration<HoganSettings>>();
            foreach (var configuration in configurations)
            {
                configuration.Configure(settings);
            }
        }
    }
}