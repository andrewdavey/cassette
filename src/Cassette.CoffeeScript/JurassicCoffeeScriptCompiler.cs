using System;
using System.Diagnostics;
using Jurassic;
using System.Linq;
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

        public CompileResult Compile(string coffeeScriptSource, CompileContext context)
        {
            Trace.Source.TraceInformation("Compiling {0}", context.SourceFilePath);
            lock (ScriptEngine) // ScriptEngine is NOT thread-safe, so we MUST lock.
            {
                try
                {
                    Trace.Source.TraceInformation("Compiled {0}", context.SourceFilePath);
                    var javascript = ScriptEngine.CallGlobalFunction<string>("compile", coffeeScriptSource);
                    return new CompileResult(javascript, Enumerable.Empty<string>());
                }
                catch (Exception ex)
                {
                    var message = ex.Message + " in " + context.SourceFilePath;
                    Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                    throw new CoffeeScriptCompileException(message, context.SourceFilePath, ex);
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

