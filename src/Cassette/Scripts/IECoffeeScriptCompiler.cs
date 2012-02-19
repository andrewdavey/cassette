using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Cassette.Interop;
using Cassette.IO;

namespace Cassette.Scripts
{
// ReSharper disable InconsistentNaming - IE == Internet Explorer
    public class IECoffeeScriptCompiler : ICompiler
// ReSharper restore InconsistentNaming
    {
        public string Compile(string source, IFile sourceFile)
        {
            Trace.Source.TraceInformation("Compiling {0}", sourceFile.FullPath);
            var workItem = new CompileTask(source);
            try
            {
                return workItem.QueueAndWaitForResult();
            }
            catch (Exception ex)
            {
                var message = ex.Message + " in " + sourceFile.FullPath;
                Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                throw new CoffeeScriptCompileException(message, sourceFile.FullPath, ex);                    
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

            public void Stop()
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
}