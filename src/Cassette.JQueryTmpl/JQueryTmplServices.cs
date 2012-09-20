using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    [ConfigurationOrder(20)]
    public class JQueryTmplServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container
                .Register(typeof(IBundlePipeline<HtmlTemplateBundle>), typeof(JQueryTmplPipeline))
                .AsMultiInstance();
        }
    }
}