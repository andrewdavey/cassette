using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace DotNet35
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsDebuggingEnabled = false;

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