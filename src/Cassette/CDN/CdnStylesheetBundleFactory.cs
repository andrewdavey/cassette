using System;
using Cassette.BundleProcessing;

namespace Cassette.CDN
{
    class CdnStylesheetBundleFactory : BundleFactoryBase<CdnStylesheetBundle>
    {
        readonly Func<IBundlePipeline<CdnStylesheetBundle>> stylesheetPipeline;

        public CdnStylesheetBundleFactory(Func<IBundlePipeline<CdnStylesheetBundle>> stylesheetPipeline)
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