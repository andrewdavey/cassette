using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    /// <summary>
    /// Inserts TypeScript reference parsing and compilation in script bundle pipelines.
    /// Replaces ParseJavaScriptReferences with ParseJavaScriptNotTypeScriptReferences.
    /// This is done because ParseJavaScriptReferences operates on JavaScript files.
    /// TypeScript at the point of use by Cassette has already been compiled down to 
    /// JavaScript but with references to TypeScript files in the header.
    /// 
    /// eg: /// <reference path="../../../../typings/jquery/jquery.d.ts" />
    /// 
    /// Swapping out ParseJavaScriptReferences for ParseJavaScriptNotTypeScriptReferences
    /// prevents these TypeScript files being included during the reference parsing.
    /// 
    /// ParseJavaScriptNotTypeScriptReferences does not otherwise affect the behaviour of
    /// ParseJavaScriptReferences.
    /// </summary>
    public class TypeScriptBundlePipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            var positionOfJavaScriptReferenceParser = pipeline.IndexOf<ParseJavaScriptReferences>();

            pipeline.RemoveAt(positionOfJavaScriptReferenceParser);
            pipeline.Insert<ParseJavaScriptNotTypeScriptReferences>(positionOfJavaScriptReferenceParser);
            return pipeline;
        }
    }
}