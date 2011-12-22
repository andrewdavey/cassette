namespace Cassette.Stylesheets
{
    class StylesheetBundleFactory : BundleFactoryBase<StylesheetBundle>
    {
        protected override StylesheetBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalStylesheetBundle(bundleDescriptor.ExternalUrl, path);
            }
            else
            {
                return new StylesheetBundle(path);
            }
        }
    }
}