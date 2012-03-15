using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public static class ScriptPipelineExtensions
    {
        public static T CompileCoffeeScript<T>(this T pipeline, ICompiler coffeeScriptCompiler)
            where T : MutablePipeline<ScriptBundle>
        {
            pipeline.InsertAfter<ParseJavaScriptReferences>(
                new ParseCoffeeScriptReferences(),
                new CompileCoffeeScript(coffeeScriptCompiler)
                );
            return pipeline;
        }

        public static T CompileCoffeeScript<T>(this T pipeline)
            where T : MutablePipeline<ScriptBundle>
        {
            pipeline.InsertAfter<ParseJavaScriptReferences>(
                new ParseCoffeeScriptReferences(),
                new CompileCoffeeScript(new JurassicCoffeeScriptCompiler())
                );
            return pipeline;
        }
    }
}