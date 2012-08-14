using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    [ProtoBuf.ProtoContract]
    public class StylesheetPipeline : MutablePipeline<StylesheetBundle>
    {
        public StylesheetPipeline()
        {
            StylesheetMinifier = new MicrosoftStylesheetMinifier();
        }

        [ProtoBuf.ProtoMember(1)]
        public IAssetTransformer StylesheetMinifier { get; set; }
       
        // TODO: Delete this property
        [ProtoBuf.ProtoMember(2)]
        public bool ConvertImageUrlsToDataUris { get; set; }

        protected override IEnumerable<IBundleProcessor<StylesheetBundle>> CreatePipeline(StylesheetBundle bundle, CassetteSettings settings)
        {
            yield return new AssignStylesheetRenderer();
            yield return new ParseCssReferences();
            if (ConvertImageUrlsToDataUris)
            {
                yield return new ConvertImageUrlsToDataUris();
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
