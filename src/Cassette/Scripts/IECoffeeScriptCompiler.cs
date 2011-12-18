using System;
using System.Diagnostics;
using Cassette.IO;

namespace Cassette.Scripts
{
    public class IECoffeeScriptCompiler : ICompiler
    {
        readonly Lazy<IEJavaScriptEngine> lazyEngine;

        public IECoffeeScriptCompiler()
        {
            lazyEngine = new Lazy<IEJavaScriptEngine>(CreateEngine);
        }

        public string Compile(string source, IFile sourceFile)
        {
            Trace.Source.TraceInformation("Compiling {0}", sourceFile.FullPath);
            var engine = lazyEngine.Value;
            lock (engine)
            {
                try
                {
                    Trace.Source.TraceInformation("Compiled {0}", sourceFile.FullPath);
                    return engine.CallFunction<string>("compile", source);
                }
                catch (Exception ex)
                {
                    var message = ex.Message + " in " + sourceFile.FullPath;
                    Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                    throw new CoffeeScriptCompileException(message, sourceFile.FullPath, ex);                    
                }
            }
        }

        static IEJavaScriptEngine CreateEngine()
        {
            var engine = new IEJavaScriptEngine();
            engine.LoadLibrary(Properties.Resources.coffeescript);
            engine.LoadLibrary("function compile(c) { return CoffeeScript.compile(c); }");
            return engine;
        }
    }
}