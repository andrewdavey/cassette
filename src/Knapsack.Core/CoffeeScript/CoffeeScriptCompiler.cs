using System;
using Jurassic;
using Jurassic.Library;

namespace Knapsack.CoffeeScript
{
    public class CoffeeScriptCompiler : ICoffeeScriptCompiler
    {
        /// <summary>
        /// ScriptEngine is expensive to create and initialize with CoffeeScript compiler,
        /// so this is done lazily.
        /// </summary>
        readonly Lazy<ScriptEngine> scriptEngine;
        readonly Func<string, string> loadSourceFromFile;

        public CoffeeScriptCompiler(Func<string, string> loadSourceFromFile)
        {
            scriptEngine = new Lazy<ScriptEngine>(CreateScriptEngineWithCoffeeScriptLoaded);
            this.loadSourceFromFile = loadSourceFromFile;
        }

        ScriptEngine CreateScriptEngineWithCoffeeScriptLoaded()
        {
            var scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.coffeescript);
            return scriptEngine;
        }

        public string CompileFile(string path)
        {
            var script = loadSourceFromFile(path);
            var callCoffeeCompile =
                "(function() { try { return CoffeeScript.compile('"
                + JavaScriptUtilities.EscapeJavaScriptString(script)
                + "'); } catch (e) { return e; } })()";
            
            object result;
            lock (ScriptEngine) // ScriptEngine is NOT thread-safe, so we MUST lock.
            {
                result = ScriptEngine.Evaluate(callCoffeeCompile);
            }
            var javascript = result as string;
            if (javascript != null)
            {
                return javascript;
            }
            else
            {
                var error = result as ErrorInstance;
                if (error != null)
                {
                    throw new CompileException(error.Message, path);
                }
                else
                {
                    throw new CompileException("Unknown CoffeeScript compilation failure.", path);
                }
            }
        }

        ScriptEngine ScriptEngine
        {
            get
            {
                return scriptEngine.Value;
            }
        }
    }
}
