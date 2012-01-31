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
            bundles.Add<StylesheetBundle>("Styles");
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            bundles.AddUrlWithAlias(
                "http://platform.twitter.com/widgets.js",
                "twitter",
                b => { b.PageLocation = "body"; b.HtmlAttributes.Add(new { async = "async" }); });
            
            bundles.AddPerSubDirectory<HtmlTemplateBundle>(
                "HtmlTemplates",
                bundle => bundle.Processor = new HtmlTemplatePipeline()
            );
        }
    }
}