using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    class StylesheetBundleFactory : BundleFactoryBase<StylesheetBundle>
    {
        readonly IBundlePipeline<StylesheetBundle> stylesheetPipeline;

        public StylesheetBundleFactory(IBundlePipeline<StylesheetBundle> stylesheetPipeline)
        {
            this.stylesheetPipeline = stylesheetPipeline;
        }

        protected override StylesheetBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalStylesheetBundle(bundleDescriptor.ExternalUrl, path)
                {
                    Processor = stylesheetPipeline
                };
            }
            else
            {
                return new StylesheetBundle(path)
                {
                    Processor = stylesheetPipeline
                };
            }
        }
    }
}