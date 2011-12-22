
namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleFactory : BundleFactoryBase<HtmlTemplateBundle>
    {
        protected override HtmlTemplateBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            return new HtmlTemplateBundle(path);
        }
    }
}