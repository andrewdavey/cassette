using System.Text.RegularExpressions;
using Cassette;
using Cassette.HtmlTemplates;
using Cassette.ModuleProcessing;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Example
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(ICassetteApplication application)
        {
            application.Add(new PerSubDirectorySource<ScriptModule>("Scripts")
            {
                FilePattern = "*.js",
                Exclude = new Regex("-vsdoc\\.js$")
            });

            application.Add(new DirectorySource<StylesheetModule>("Styles")
            {
                FilePattern = "*.css;*.less"
            });

            application.Add(new DirectorySource<HtmlTemplateModule>("HtmlTemplates"));

            // TODO: Customize pipelines (e.g. compile LESS)
        }
    }
}