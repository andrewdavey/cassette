using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    [ConfigurationOrder(20)]
    public class HoganServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<HoganSettings>().AsSingleton();

            container
                .Register(typeof(IBundlePipeline<HtmlTemplateBundle>), typeof(HoganPipeline))
                .AsMultiInstance();
        }
    }
}