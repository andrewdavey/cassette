using System;
using Cassette.Utilities;
using Jurassic;
using Jurassic.Library;

namespace Cassette.Compilation
{
    public class CoffeeScriptCompiler : ICompiler
    {
        public CoffeeScriptCompiler()
        {
            //ScriptEngine is expensive to create and initialize with CoffeeScript compiler, so this is done lazily.
            scriptEngine = new Lazy<ScriptEngine>(CreateScriptEngineWithCoffeeScriptLoaded);
        }

        readonly Lazy<ScriptEngine> scriptEngine;

        public string Compile(string coffeeScriptSource, string filename, IFileSystem currentDirectory)
        {
            var callCoffeeCompile =
                "(function() { try { return CoffeeScript.compile('"
                + JavaScriptUtilities.EscapeJavaScriptString(coffeeScriptSource)
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
                    throw new CompileException(error.Message + " in " + filename, filename);
                }
                else
                {
                    throw new CompileException("Unknown CoffeeScript compilation failure.", filename);
                }
            }
        }

        ScriptEngine CreateScriptEngineWithCoffeeScriptLoaded()
        {
            var scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.coffeescript);
            return scriptEngine;
        }

        ScriptEngine ScriptEngine
        {
            get { return scriptEngine.Value; }
        }
    }
}
