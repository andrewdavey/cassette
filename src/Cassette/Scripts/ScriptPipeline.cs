using Cassette.BundleProcessing;
using TinyIoC;

namespace Cassette.Scripts
{
    public class ScriptPipeline : BundlePipeline<ScriptBundle>
    {
        public ScriptPipeline(TinyIoCContainer container, CassetteSettings settings)
            : base(container)
        {
            AddRange(new IBundleProcessor<ScriptBundle>[]
            {
                container.Resolve<AssignScriptRenderer>(),
                new ParseJavaScriptReferences(),
                new SortAssetsByDependency(),
                new AssignHash()
            });

            if (!settings.IsDebuggingEnabled)
            {
                Add(new ConcatenateAssets { Separator = ";" });
                var minifier = container.Resolve<IJavaScriptMinifier>();
                Add(new MinifyAssets(minifier));
            }
        }
    }
}