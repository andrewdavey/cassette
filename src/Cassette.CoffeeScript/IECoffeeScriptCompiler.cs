#if !NET35
using System;
using System.Diagnostics;
using System.Linq;
using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Scripts
{
// ReSharper disable InconsistentNaming - IE == Internet Explorer
    public class IECoffeeScriptCompiler : ICoffeeScriptCompiler
// ReSharper restore InconsistentNaming
    {
        readonly IECoffeeScriptCompilationQueue compilationQueue;

        public IECoffeeScriptCompiler(IECoffeeScriptCompilationQueue compilationQueue)
        {
            this.compilationQueue = compilationQueue;
        }

        public CompileResult Compile(string source, CompileContext context)
        {
            Trace.Source.TraceInformation("Compiling {0}", context.SourceFilePath);
            var workItem = compilationQueue.EnqueueCompilation(source);
            try
            {
                var javascript = workItem.AwaitResult();
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
}
#endif