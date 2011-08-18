using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : IModuleProcessor<StylesheetModule>
    {
        public StylesheetPipeline()
        {
            StylesheetMinifier = new MicrosoftStyleSheetMinifier();
        }

        public IAssetTransformer StylesheetMinifier { get; set; }
        public bool CompileLess { get; set; }
        public bool ConvertImageUrlsToDataUris { get; set; }

        public void Process(StylesheetModule module, ICassetteApplication application)
        {
            foreach (var processor in CreatePipeline(application))
            {
                processor.Process(module, application);
            }
        }

        IEnumerable<IModuleProcessor<StylesheetModule>> CreatePipeline(ICassetteApplication application)
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
            yield return new ExpandCssUrls();
            yield return new SortAssetsByDependency();
            if (application.IsOutputOptimized)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(StylesheetMinifier);
            }
        }
    }
}
