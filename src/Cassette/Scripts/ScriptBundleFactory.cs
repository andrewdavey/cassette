using Cassette.Configuration;
namespace Cassette.Scripts
{
    class ScriptBundleFactory : BundleFactoryBase<ScriptBundle>
    {
        readonly CassetteSettings settings;

        public ScriptBundleFactory(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override ScriptBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            var pipeline = settings.GetDefaults<ScriptBundle>().BundlePipeline;
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalScriptBundle(
                    bundleDescriptor.ExternalUrl,
                    path,
                    bundleDescriptor.FallbackCondition
                )
                {
                    Processor = pipeline
                };
            }
            else
            {
                return new ScriptBundle(path)
                {
                    Processor = pipeline
                };
            }
        }
    }
}