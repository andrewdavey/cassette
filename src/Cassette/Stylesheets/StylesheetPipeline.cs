using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : MutablePipeline<StylesheetModule>
    {
        public StylesheetPipeline()
        {
            StylesheetMinifier = new MicrosoftStyleSheetMinifier();
            CompileLess = true;
        }

        public IAssetTransformer StylesheetMinifier { get; set; }
        public bool CompileLess { get; set; }
        public bool ConvertImageUrlsToDataUris { get; set; }

        protected override IEnumerable<IModuleProcessor<StylesheetModule>> CreatePipeline(StylesheetModule module, ICassetteApplication application)
        {
            yield return new ParseCssReferences();
            if (CompileLess)
            {
                yield return new ParseLessReferences();
                yield return new CompileLess(new LessCompiler());
            }
            if (ConvertImageUrlsToDataUris)
            {
                yield return new AddTransformerToAssets(new DataUriGenerator());
            }
            yield return new SortAssetsByDependency();
            if (application.IsOutputOptimized)
            {
                yield return new ExpandCssUrls();
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(StylesheetMinifier);
            }
        }
    }
}