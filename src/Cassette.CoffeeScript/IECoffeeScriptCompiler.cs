using System;
using System.Diagnostics;
using System.Linq;
using Cassette.Interop;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Scripts
{

#if !NET35
    using System.Collections.Concurrent;
    using System.Threading;

// ReSharper disable InconsistentNaming - IE == Internet Explorer
    public class IECoffeeScriptCompiler : ICoffeeScriptCompiler
// ReSharper restore InconsistentNaming
    {
        public CompileResult Compile(string source, CompileContext context)
        {
            Trace.Source.TraceInformation("Compiling {0}", context.SourceFilePath);
            var workItem = new CompileTask(source);
            try
            {
                var javascript = workItem.QueueAndWaitForResult();
                return new CompileResult(javascript, Enumerable.Empty<string>());
            }
            catch (Exception ex)
            {
                var message = ex.Message + " in " + context.SourceFilePath;
                Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                throw new CoffeeScriptCompileException(message, context.SourceFilePath, ex);                    
            }
        }

        // The IE script engine COM object should always be called from the same thread.
        // This worker object keeps a single thread alive (in a loop) and processes
        // CompileTask objects one at a time.

        internal class SingleThreadedWorker
        {
            public static readonly SingleThreadedWorker Singleton = new SingleThreadedWorker();

            bool stop;
            readonly Lazy<ConcurrentQueue<CompileTask>> lazyQueue;

            SingleThreadedWorker()
            {
                lazyQueue = new Lazy<ConcurrentQueue<CompileTask>>(Start);
            }

            ConcurrentQueue<CompileTask> Start()
            {
                var thread = new Thread(Loop)
                {
                    IsBackground = true
                };
                thread.Start();
                return new ConcurrentQueue<CompileTask>();
            }

            public void QueueWorkItem(CompileTask item)
            {
                lazyQueue.Value.Enqueue(item);
            }

            void Loop()
            {
                var engine = CreateEngine();
                while (!stop)
                {
                    CompileTask item;
                    if (!lazyQueue.Value.TryDequeue(out item))
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    item.Execute(engine);
                }
                engine.Dispose();
            }

            // TODO: Call Stop when app shutting down?
            void Stop()
            {
                stop = true;
            }

            IEJavaScriptEngine CreateEngine()
            {
                var engine = new IEJavaScriptEngine();
                engine.Initialize();
                engine.LoadLibrary(Properties.Resources.coffeescript);
                engine.LoadLibrary(@"function compile(c) {
    try {
        return { output: CoffeeScript.compile(c), error: '' };
    } catch (e) {
        return { error: e.message };
    }
}");
                return engine;
            }
        }

        internal class CompileTask
        {
            readonly ManualResetEventSlim gate = new ManualResetEventSlim();
            readonly string source;
            string result;
            Exception exception;

            public CompileTask(string source)
            {
                this.source = source;
            }

            public string QueueAndWaitForResult()
            {
                SingleThreadedWorker.Singleton.QueueWorkItem(this);
                WaitToBeExecuted();

                if (exception != null) throw exception;

                return result;
            }

            void WaitToBeExecuted()
            {
                gate.Wait();
            }

            public void Execute(IEJavaScriptEngine engine)
            {
                CompileSource(engine);
                FinishedExecuting();
            }

            void CompileSource(IEJavaScriptEngine engine)
            {
                try
                {
                    var compileResult = engine.CallFunction<dynamic>("compile", source);
                    if (compileResult.error != "")
                    {
                        throw new Exception(compileResult.error);
                    }
                    else
                    {
                        result = compileResult.output;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            void FinishedExecuting()
            {
                gate.Set();
            }
        }
    }
#endif

#if NET35
    public class IECoffeeScriptCompiler : ICompiler
    {
        readonly Lazy<IEJavaScriptEngine> lazyEngine;

        public IECoffeeScriptCompiler()
        {
            lazyEngine = new Lazy<IEJavaScriptEngine>(CreateEngine);
        }

        public CompileResult Compile(string source, CompileContext context)
        {
            Trace.Source.TraceInformation("Compiling {0}", context.SourceFilePath);
            var engine = lazyEngine.Value;
            lock (engine)
            {
                try
                {
                    Trace.Source.TraceInformation("Compiled {0}", context.SourceFilePath);
                    var javascript = engine.CallFunction<string>("compile", source);
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

        static IEJavaScriptEngine CreateEngine()
        {
            var engine = new IEJavaScriptEngine();
            engine.Initialize();
            engine.LoadLibrary(Properties.Resources.coffeescript);
            engine.LoadLibrary("function compile(c) { return CoffeeScript.compile(c); }");
            return engine;
        }
    }
#endif
}