using System.Text.RegularExpressions;
using Cassette;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Example
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(ModuleConfiguration modules)
        {
            modules.Add(
                new PerSubDirectorySource<ScriptModule>("Scripts")
                {
                    FilePattern = "*.js",
                    Exclude = new Regex("-vsdoc\\.js$")
                },
                new ExternalScriptModule("twitter", "http://platform.twitter.com/widgets.js")
                {
                    Location = "body"
                }
            );

            modules.Add(new PerSubDirectorySource<StylesheetModule>("Styles")
            {
                FilePattern = "*.css;*.less",
                CustomizeModule = module => module.Processor = new StylesheetPipeline
                {
                    CompileLess = true,
                    ConvertImageUrlsToDataUris = true
                }
            });

            modules.Add(new PerSubDirectorySource<HtmlTemplateModule>("HtmlTemplates")
            {
                CustomizeModule = module => module.Processor = new KnockoutJQueryTmplPipeline()
            });
        }
    }
}