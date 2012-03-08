﻿using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : MutablePipeline<StylesheetBundle>
    {
        public StylesheetPipeline()
        {
            StylesheetMinifier = new MicrosoftStylesheetMinifier();
            CompileLess = true;
            CompileSass = true;
        }

        public IAssetTransformer StylesheetMinifier { get; set; }
        public bool CompileLess { get; set; }
        public bool CompileSass { get; set; }
        // TODO: Obselete this property in next version
        // Use the EmbedImages extension method instead.
        public bool ConvertImageUrlsToDataUris { get; set; }

        protected override IEnumerable<IBundleProcessor<StylesheetBundle>> CreatePipeline(StylesheetBundle bundle, CassetteSettings settings)
        {
            yield return new AssignStylesheetRenderer();
            yield return new ParseCssReferences();
            if (CompileLess)
            {
                yield return new ParseLessReferences();
                yield return new CompileLess(new LessCompiler());
            }
            if (CompileSass)
            {
                yield return new ParseSassReferences();
                yield return new CompileSass(new SassCompiler());
            }
            if (ConvertImageUrlsToDataUris)
            {
                yield return new ConvertImageUrlsToDataUris(true); // Default is to support IE8
            }
            yield return new ExpandCssUrls();
            yield return new SortAssetsByDependency();
            yield return new AssignHash();
            if (!settings.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(StylesheetMinifier);
            }
        }
    }
}
