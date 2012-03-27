using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleFactory : BundleFactoryBase<HtmlTemplateBundle>
    {
        readonly IBundlePipeline<HtmlTemplateBundle> bundlePipeline;

        public HtmlTemplateBundleFactory(IBundlePipeline<HtmlTemplateBundle> bundlePipeline)
        {
            this.bundlePipeline = bundlePipeline;
        }

        protected override HtmlTemplateBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            return new HtmlTemplateBundle(path)
            {
                Processor = bundlePipeline
            };
        }
    }
}