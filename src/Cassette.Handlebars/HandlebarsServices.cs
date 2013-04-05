using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    [ConfigurationOrder(20)]
    public class HandlebarsServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<HandlebarsSettings>().AsSingleton();

            container
                .Register(typeof(IBundlePipeline<HtmlTemplateBundle>), typeof(HandlebarsPipeline))
                .AsMultiInstance();
        }
    }
}