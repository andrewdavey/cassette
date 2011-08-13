using System.Text.RegularExpressions;
using Cassette;
using Cassette.Compilation;
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
                .ExcludeFiles(new Regex("-vsdoc\\.js$"))
                .ProcessWith(new DefaultScriptPipeline());

            application.HasModules<StylesheetModule>()
                .Directories("Styles")
                .IncludeFiles("*.css", "*.less")
                .ProcessWith(
                    new ParseCssReferences(),
                    new ParseLessReferences(),
                    new SortAssetsByDependency(),
                    new CompileLess(new LessCompiler()),
                    new ExpandCssUrls(),
                    new ConditionalStep<StylesheetModule>(
                        (m, a) => a.IsOutputOptimized,
                        new ConcatenateAssets(),
                        new MinifyAssets(new MicrosoftStyleSheetMinifier())
                    )
                );
            application.HasModules<HtmlTemplateModule>()
                .ForSubDirectoriesOf("HtmlTemplates")
                .ProcessWith(
                    new WrapHtmlTemplatesInScriptBlocks(),
                    new ConcatenateAssets()
                );
        }
    }
}