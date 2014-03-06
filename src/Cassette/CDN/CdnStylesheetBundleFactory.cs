using System;
using Cassette.BundleProcessing;
using Cassette.Stylesheets;

namespace Cassette.CDN
{
    class CdnStylesheetBundleFactory : BundleFactoryBase<CdnStylesheetBundle>
    {
        readonly Func<IBundlePipeline<StylesheetBundle>> stylesheetPipeline;

        public CdnStylesheetBundleFactory(Func<IBundlePipeline<StylesheetBundle>> stylesheetPipeline)
        {
            this.stylesheetPipeline = stylesheetPipeline;
        }

        protected override CdnStylesheetBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            return new CdnStylesheetBundle(bundleDescriptor.ExternalUrl, path)
            {
                Pipeline = stylesheetPipeline()
            };
        }
    }
}