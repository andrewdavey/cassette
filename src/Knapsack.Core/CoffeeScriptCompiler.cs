using System;
using Jurassic;
using System.IO;
using Jurassic.Library;

namespace Knapsack
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

        public string CompileCoffeeScript(string path)
        {
            var script = loadSourceFromFile(path);
            var callCoffeeCompile =
                "(function() { try { return CoffeeScript.compile('"
                + EscapeJavaScriptString(script)
                + "'); } catch (e) { return e; } })()";
            
            object result;
            lock (ScriptEngine) // ScriptEngine is NOT thread-safe, so we MUST lock.
            {
                result = ScriptEngine.Evaluate(callCoffeeCompile);
            }
            var javascript = result as string;
            if (javascript == null)
            {
                var error = result as ErrorInstance;
                if (error != null)
                {
                    return "alert('CoffeeScript compile error in "
                        + EscapeJavaScriptString(path)
                        + "\\r\\n"
                        + EscapeJavaScriptString(error.Message)
                        + "');";
                    // TODO: Configure how to handle the compile error. For example:
                    // throw new Exception("Cannot compile coffeescript.");
                }
                else
                {
                    return "alert('CoffeeScript compile error in "
                        + EscapeJavaScriptString(path);
                }
            }
            else
            {
                return javascript;
            }
        }

        string EscapeJavaScriptString(string sourceCode)
        {
            return sourceCode
                    .Replace(@"\", @"\\")
                    .Replace("'", @"\'")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");
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
