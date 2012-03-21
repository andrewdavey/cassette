using System.IO;
using Cassette.HtmlTemplates;

namespace Cassette.Configuration
{
    [InternalCassetteConfiguration]
    public class HtmlTemplateBundleConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.ModifyDefaults<HtmlTemplateBundle>(defaults =>
            {
                defaults.FileSearch = new FileSearch
                {
                    Pattern = "*.htm;*.html",
                    SearchOption = SearchOption.AllDirectories
                };

                defaults.BundleFactory = new HtmlTemplateBundleFactory(settings);
                
                defaults.BundlePipeline = new HtmlTemplatePipeline();
            });
        }
    }
}