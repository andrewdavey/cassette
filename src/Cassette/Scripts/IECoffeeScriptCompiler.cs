using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        	readonly BlockingCollection<CompileTask> queue;
			
			SingleThreadedWorker()
            {
                queue = new BlockingCollection<CompileTask>();
            }

            ConcurrentQueue<CompileTask> Start()
            {
            	Task.Factory.StartNew(Loop);
                return new ConcurrentQueue<CompileTask>();
            }

            public void QueueWorkItem(CompileTask item)
            {
                queue.Add(item);
            }

            void Loop()
            {
                var engine = CreateEngine();

				foreach (var item in queue.GetConsumingEnumerable())
				{
					item.Execute(engine);
				}

                engine.Dispose();
            }

            public void Stop()
            {
                queue.CompleteAdding();
            }

            IEJavaScriptEngine CreateEngine()
            {
                var engine = new IEJavaScriptEngine();
                engine.Initialize();
                engine.LoadLibrary(Properties.Resources.coffeescript);
                engine.LoadLibrary("function compile(c) { return CoffeeScript.compile(c); }");
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
                    result = engine.CallFunction<string>("compile", source);
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