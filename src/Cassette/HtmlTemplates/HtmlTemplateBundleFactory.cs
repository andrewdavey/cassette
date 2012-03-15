
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleFactory : BundleFactoryBase<HtmlTemplateBundle>
    {
        readonly CassetteSettings settings;

        public HtmlTemplateBundleFactory(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override HtmlTemplateBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            return new HtmlTemplateBundle(path)
            {
                Processor = settings.GetDefaultBundleProcessor<HtmlTemplateBundle>()
            };
        }
    }
}