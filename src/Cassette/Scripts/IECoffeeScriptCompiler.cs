using System;
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
            var engine = lazyEngine.Value;
            lock (engine)
            {
                try
                {
                    return engine.CallFunction<string>("compile", source);
                }
                catch (Exception ex)
                {
                    throw new CoffeeScriptCompileException(ex.Message + " in " + sourceFile.FullPath, sourceFile.FullPath, ex);                    
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