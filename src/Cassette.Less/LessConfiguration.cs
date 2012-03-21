using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    [InternalCassetteConfiguration]
    public class LessConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.ModifyDefaults<StylesheetBundle>(defaults =>
            {
                defaults.FileSearch.Pattern += ";*.less";
                defaults.BundlePipeline.CompileLess();
            });
        }
    }
}