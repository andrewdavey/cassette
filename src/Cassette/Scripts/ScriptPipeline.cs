using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ScriptPipeline : BundlePipeline<ScriptBundle>
    {
        public ScriptPipeline(IJavaScriptMinifier javaScriptMinifier, IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<ScriptBundle>[]
            {
                new AssignScriptRenderer(urlGenerator),
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