using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    [InternalCassetteConfiguration]
    public class SassConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.ModifyDefaults<StylesheetBundle>(defaults =>
            {
                defaults.FileSearch.Pattern += ";*.scss;*.sass";
                defaults.BundlePipeline.CompileSass();
            });
        }
    }
}