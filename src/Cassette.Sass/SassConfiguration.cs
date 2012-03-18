using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    [InternalCassetteConfiguration]
    public class SassConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.AddDefaultFileSearchPattern<StylesheetBundle>("*.scss;*.sass");
        }
    }
}