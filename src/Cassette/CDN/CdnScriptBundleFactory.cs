using System;
using Cassette.BundleProcessing;

namespace Cassette.CDN
{
    class CdnScriptBundleFactory : BundleFactoryBase<CdnScriptBundle>
    {
        readonly Func<IBundlePipeline<CdnScriptBundle>> scriptPipeline;

        public CdnScriptBundleFactory(Func<IBundlePipeline<CdnScriptBundle>> scriptPipeline)
        {
            this.scriptPipeline = scriptPipeline;
        }

        protected override CdnScriptBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            return new CdnScriptBundle(
                bundleDescriptor.ExternalUrl,
                path,
                bundleDescriptor.FallbackCondition
            )
            {
                Pipeline = scriptPipeline()
            };
        }
    }
}