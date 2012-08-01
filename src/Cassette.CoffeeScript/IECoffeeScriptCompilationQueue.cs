#if !NET35
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Cassette.Interop;

namespace Cassette.Scripts
{
    public class IECoffeeScriptCompilationQueue : IDisposable
    {
        readonly BlockingCollection<CompileTask> compileTasks = new BlockingCollection<CompileTask>();

        public void Start()
        {
            Task.Factory.StartNew(Loop);
        }

        internal CompileTask EnqueueCompilation(string source)
        {
            var task = new CompileTask(source);
            compileTasks.Add(task);
            return task;
        }

        void Loop()
        {
            using (var engine = CreateEngine())
            {
                foreach (var compileTask in compileTasks.GetConsumingEnumerable())
                {
                    compileTask.Execute(engine);
                }
            }
        }

        IEJavaScriptEngine CreateEngine()
        {
            var engine = new IEJavaScriptEngine();
            engine.Initialize();
            engine.LoadLibrary(Properties.Resources.coffeescript);
            engine.LoadLibrary(
                @"function compile(c) {
    try {
        return { output: CoffeeScript.compile(c), error: '' };
    } catch (e) {
        return { error: e.message };
    }
}");
            return engine;
        }

        public void Dispose()
        {
            compileTasks.CompleteAdding();
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

            public string AwaitResult()
            {
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
#endif