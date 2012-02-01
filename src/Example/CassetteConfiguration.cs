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
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            bundles.AddUrlWithAlias("http://platform.twitter.com/widgets.js", "twitter", b => b.PageLocation = "body");
            
            bundles.AddPerSubDirectory<HtmlTemplateBundle>(
                "HtmlTemplates",
                bundle => bundle.Processor = new HtmlTemplatePipeline()
            );

            bundles.Add<StylesheetBundle>("Styles", b => b.Processor = new StylesheetPipeline()
                .EmbedImages(whitelistPath => whitelistPath.Contains("/embed/"))
                .EmbedFonts(whitelistPath => whitelistPath.Contains("/embed/")));
        }
    }
}