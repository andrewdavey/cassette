using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Example
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            //bundles.Add<StylesheetBundle>("Styles");
            //bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            //bundles.AddUrlWithAlias("http://platform.twitter.com/widgets.js", "twitter", b => b.PageLocation = "body");
            
            //bundles.AddPerSubDirectory<HtmlTemplateBundle>(
            //    "HtmlTemplates",
            //    bundle => bundle.Processor = new HtmlTemplatePipeline()
            //);
            
            bundles.Add<StylesheetBundle>(
                "styles/embeddables/images",
                (bundle) => bundle.Processor = new StylesheetPipeline
                {
                    ConvertImageUrlsToDataUris = true,
                    ConvertFontUrlsToDataUris = true
                }
            );
        }
    }
}