using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ScriptPipeline : BundlePipeline<ScriptBundle>
    {
        public ScriptPipeline()
            : this(new MicrosoftJavaScriptMinifier())
        {
        }

        public ScriptPipeline(IAssetTransformer javaScriptMinifier)
        {
            AddRange(new IBundleProcessor<ScriptBundle>[]
            {
                new AssignScriptRenderer(),
                new ParseJavaScriptReferences(),
                new SortAssetsByDependency(),
                new AssignHash(),
                new ConditionalBundlePipeline<ScriptBundle>(
                    settings => !settings.IsDebuggingEnabled,
                    new ConcatenateAssets(),
                    new MinifyAssets(javaScriptMinifier)
                )
            });
        }
    }
}