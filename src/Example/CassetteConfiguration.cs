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
            application.HasModules<ScriptModule>()
                .ForSubDirectoriesOf("Scripts")
                .IncludeFiles("*.js")
                .ExcludeFiles(new Regex("-vsdoc\\.js$"));

            application.HasModules<StylesheetModule>()
                .Directories("Styles")
                .IncludeFiles("*.css", "*.less")
                .ProcessWith(new DefaultStylesheetPipeline { CompileLess = true });

            application.HasModules<HtmlTemplateModule>()
                .ForSubDirectoriesOf("HtmlTemplates");
        }
    }
}