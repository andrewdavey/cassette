using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptPipeline : MutablePipeline<ScriptBundle>
    {
        public ScriptPipeline()
        {
            Minifier = new MicrosoftJavaScriptMinifier();
        }

        public IAssetTransformer Minifier { get; set; }

        protected override IEnumerable<IBundleProcessor<ScriptBundle>> CreatePipeline(ScriptBundle bundle, CassetteSettings settings)
        {
            yield return new AssignScriptRenderer();
            yield return new ParseJavaScriptReferences();
            yield return new SortAssetsByDependency();
            yield return new AssignHash();
            if (!settings.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(Minifier);
            }
        }
    }
}