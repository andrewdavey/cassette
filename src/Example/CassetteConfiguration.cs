using System.Text.RegularExpressions;
using Cassette;
using Cassette.ModuleProcessing;

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
                .ProcessWith(
                    new ParseJavaScriptReferences(),
                    new SortAssetsByDependency(),
                    new ConditionalStep<ScriptModule>(
                        (m,a) => a.IsOutputOptimized,
                        new ConcatenateAssets(),
                        new MinifyAssets(new MicrosoftStyleSheetMinifier())
                    )
                );
            application.HasModules<StylesheetModule>()
                .Directories("Styles")
                .ProcessWith(
                    new ParseCssReferences(),
                    new SortAssetsByDependency(),
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