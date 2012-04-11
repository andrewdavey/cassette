using System;
using System.Diagnostics;

namespace Cassette.Web
{
    /// <summary>
    /// Collects all of Cassette's trace output during start-up.
    /// </summary>
    class StartUpTraceRecorder : IDisposable
    {
        readonly Stopwatch startupTimer;
        readonly TraceListener startupTraceListener;

        public StartUpTraceRecorder()
        {
            startupTraceListener = CreateTraceListener();
            Trace.Source.Listeners.Add(startupTraceListener);
            startupTimer = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (startupTimer == null) return;

            startupTimer.Stop();
            Trace.Source.Listeners.Remove(startupTraceListener);
        }

        public string TraceOutput
        {
            get
            {
                return string.Format(
                    "{0}{1}Total time elapsed: {2}ms",
                    startupTraceListener,
                    Environment.NewLine,
                    startupTimer.ElapsedMilliseconds
                );
            }
        }

        StringBuilderTraceListener CreateTraceListener()
        {
            return new StringBuilderTraceListener
            {
                Filter = new EventTypeFilter(SourceLevels.All)
            };
        }
    }
}