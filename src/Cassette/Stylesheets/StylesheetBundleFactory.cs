using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    class StylesheetBundleFactory : BundleFactoryBase<StylesheetBundle>
    {
        readonly Func<IBundlePipeline<StylesheetBundle>> stylesheetPipeline;

        public StylesheetBundleFactory(Func<IBundlePipeline<StylesheetBundle>> stylesheetPipeline)
        {
            this.stylesheetPipeline = stylesheetPipeline;
        }

        protected override StylesheetBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalStylesheetBundle(bundleDescriptor.ExternalUrl, path)
                {
                    Pipeline = stylesheetPipeline()
                };
            }
            else
            {
                return new StylesheetBundle(path)
                {
                    Pipeline = stylesheetPipeline()
                };
            }
        }
    }
}