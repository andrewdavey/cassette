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
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalStylesheetBundle(bundleDescriptor.ExternalUrl, path)
                {
                    Processor = settings.GetDefaultBundleProcessor<StylesheetBundle>()
                };
            }
            else
            {
                return new StylesheetBundle(path)
                {
                    Processor = settings.GetDefaultBundleProcessor<StylesheetBundle>()
                };
            }
        }
    }
}