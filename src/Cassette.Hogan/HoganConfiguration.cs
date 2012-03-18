using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    [InternalCassetteConfiguration]
    public class HoganConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.AddDefaultFileSearchPattern<HtmlTemplateBundle>("*.mustache;*.jst;*.tmpl");
        }
    }
}