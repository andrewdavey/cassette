using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    [InternalCassetteConfiguration]
    public class LessConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.AddDefaultFileSearchPattern<StylesheetBundle>("*.less");
        }
    }
}