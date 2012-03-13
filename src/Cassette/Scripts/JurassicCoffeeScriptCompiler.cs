using System;
using System.Diagnostics;
using Cassette.IO;
using Jurassic;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Scripts
{
    public class JurassicCoffeeScriptCompiler : ICompiler
    {
        static JurassicCoffeeScriptCompiler()
        {
            LazyScriptEngine = new Lazy<ScriptEngine>(CreateScriptEngineWithCoffeeScriptLoaded);
        }

        readonly static Lazy<ScriptEngine> LazyScriptEngine;

        public string Compile(string coffeeScriptSource, IFile sourceFile)
        {
            Trace.Source.TraceInformation("Compiling {0}", sourceFile.FullPath);
            lock (ScriptEngine) // ScriptEngine is NOT thread-safe, so we MUST lock.
            {
                try
                {
                    Trace.Source.TraceInformation("Compiled {0}", sourceFile.FullPath);
                    return ScriptEngine.CallGlobalFunction<string>("compile", coffeeScriptSource);
                }
                catch (Exception ex)
                {
                    var message = ex.Message + " in " + sourceFile.FullPath;
                    Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                    throw new CoffeeScriptCompileException(message, sourceFile.FullPath, ex);
                }
            }
        }

        static ScriptEngine CreateScriptEngineWithCoffeeScriptLoaded()
        {
            var engine = new ScriptEngine();
            engine.Execute(Properties.Resources.coffeescript);
            engine.Execute("function compile(c) { return CoffeeScript.compile(c); }");
            return engine;
        }

        ScriptEngine ScriptEngine
        {
            get { return LazyScriptEngine.Value; }
        }
    }
}

