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
            // Enable compilation of LESS files:
            settings.SetDefaultBundleProcessor(new StylesheetPipeline().CompileLess());

            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            bundles.AddUrlWithAlias(
                "http://platform.twitter.com/widgets.js",
                "twitter",
                b =>
                {
                    b.PageLocation = "body";
                    b.HtmlAttributes.Add(new { async = "async" });
                });
            
            bundles.AddPerSubDirectory<HtmlTemplateBundle>("HtmlTemplates");
            bundles.Add<StylesheetBundle>("Styles");
        }
    }
}