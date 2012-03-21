using System.IO;
using Cassette.Stylesheets;

namespace Cassette.Configuration
{
    [InternalCassetteConfiguration]
    public class StylesheetBundleConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.ModifyDefaults<StylesheetBundle>(defaults =>
            {
                defaults.FileSearch = new FileSearch
                {
                    Pattern = "*.css",
                    SearchOption = SearchOption.AllDirectories
                };

                defaults.BundleFactory = new StylesheetBundleFactory(settings);

                defaults.BundlePipeline = new StylesheetPipeline();
            });
        }
    }
}