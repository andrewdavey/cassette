using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public static class ScriptPipelineExtensions
    {
        public static T CompileCoffeeScript<T>(this T pipeline, ICompiler coffeeScriptCompiler)
            where T : IBundlePipeline<ScriptBundle>
        {
            pipeline.InsertAfter<ParseJavaScriptReferences, ScriptBundle>(
                new ParseCoffeeScriptReferences(),
                new CompileCoffeeScript(coffeeScriptCompiler)
            );
            return pipeline;
        }

        public static T CompileCoffeeScript<T>(this T pipeline)
            where T : IBundlePipeline<ScriptBundle>
        {
            return CompileCoffeeScript(pipeline, new JurassicCoffeeScriptCompiler());
        }
    }
}