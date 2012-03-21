using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    [InternalCassetteConfiguration]
    public class HoganConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.ModifyDefaults<HtmlTemplateBundle>(defaults =>
            {
                defaults.FileSearch.Pattern += ";*.mustache;*.jst;*.tmpl";
                defaults.BundlePipeline = new HoganPipeline();
            });
        }
    }
}