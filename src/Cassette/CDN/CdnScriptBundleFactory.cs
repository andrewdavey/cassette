using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;

namespace Cassette.CDN
{
    class CdnScriptBundleFactory : BundleFactoryBase<CdnScriptBundle>
    {
        readonly Func<IBundlePipeline<ScriptBundle>> scriptPipeline;

        public CdnScriptBundleFactory(Func<IBundlePipeline<ScriptBundle>> scriptPipeline)
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