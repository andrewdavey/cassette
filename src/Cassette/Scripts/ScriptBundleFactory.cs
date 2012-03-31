using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    class ScriptBundleFactory : BundleFactoryBase<ScriptBundle>
    {
        readonly IBundlePipeline<ScriptBundle> scriptPipeline;

        public ScriptBundleFactory(IBundlePipeline<ScriptBundle> scriptPipeline)
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
                    Processor = scriptPipeline
                };
            }
            else
            {
                return new ScriptBundle(path)
                {
                    Processor = scriptPipeline
                };
            }
        }
    }
}