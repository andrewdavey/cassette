using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    class StylesheetBundleFactory : BundleFactoryBase<StylesheetBundle>
    {
        readonly CassetteSettings settings;

        public StylesheetBundleFactory(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override StylesheetBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            var pipeline = settings.GetDefaults<StylesheetBundle>().BundlePipeline;
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalStylesheetBundle(bundleDescriptor.ExternalUrl, path)
                {
                    Processor = pipeline
                };
            }
            else
            {
                return new StylesheetBundle(path)
                {
                    Processor = pipeline
                };
            }
        }
    }
}