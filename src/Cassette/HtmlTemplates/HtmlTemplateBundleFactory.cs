using System;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleFactory : BundleFactoryBase<HtmlTemplateBundle>
    {
        readonly Func<IBundlePipeline<HtmlTemplateBundle>> bundlePipeline;

        public HtmlTemplateBundleFactory(Func<IBundlePipeline<HtmlTemplateBundle>> bundlePipeline)
        {
            this.bundlePipeline = bundlePipeline;
        }

        protected override HtmlTemplateBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            return new HtmlTemplateBundle(path)
            {
                Pipeline = bundlePipeline()
            };
        }
    }
}