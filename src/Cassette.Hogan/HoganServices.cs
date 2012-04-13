using Cassette.BundleProcessing;
using TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class HoganServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register((c, p) => CreateSettings(c));

            container
                .Register(typeof(IBundlePipeline<HtmlTemplateBundle>), typeof(HoganPipeline))
                .AsMultiInstance();
        }

        HoganSettings settings;
        readonly object settingsCreationLock = new object();

        HoganSettings CreateSettings(TinyIoCContainer container)
        {
            lock (settingsCreationLock)
            {
                if (settings != null) return settings;

                var configurations = container.ResolveAll<IConfiguration<HoganSettings>>();
                settings = new HoganSettings();
                foreach (var configuration in configurations)
                {
                    configuration.Configure(settings);
                }
                return settings;
            }
        }
    }
}