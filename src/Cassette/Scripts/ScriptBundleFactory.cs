using System;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    class ScriptBundleFactory : BundleFactoryBase<ScriptBundle>
    {
        readonly Func<IBundlePipeline<ScriptBundle>> scriptPipeline;

        public ScriptBundleFactory(Func<IBundlePipeline<ScriptBundle>> scriptPipeline)
        {
            this.scriptPipeline = scriptPipeline;
        }

        protected override ScriptBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalScriptBundle(
                    bundleDescriptor.ExternalUrl,
                    path,
                    bundleDescriptor.FallbackCondition
                )
                {
                    Pipeline = scriptPipeline()
                };
            }
            else
            {
                return new ScriptBundle(path)
                {
                    Pipeline = scriptPipeline()
                };
            }
        }
    }
}