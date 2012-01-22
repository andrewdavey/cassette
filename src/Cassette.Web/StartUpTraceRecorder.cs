using System;
using System.Diagnostics;

namespace Cassette.Web
{
    /// <summary>
    /// Collects all of Cassette's trace output during start-up.
    /// </summary>
    class StartUpTraceRecorder
    {
        Stopwatch startupTimer;
        TraceListener startupTraceListener;

        public void Start()
        {
            RequireNotStarted();

            startupTraceListener = CreateTraceListener();
            Trace.Source.Listeners.Add(startupTraceListener);
            startupTimer = Stopwatch.StartNew();
        }

        public void Stop()
        {
            if (startupTimer == null) return;

            startupTimer.Stop();
            Trace.Source.Listeners.Remove(startupTraceListener);
            startupTimer = null;
        }

        public long ElapsedMilliseconds
        {
            get { return startupTimer.ElapsedMilliseconds; }
        }

        public string TraceOutput
        {
            get { return startupTraceListener.ToString(); }
        }

        void RequireNotStarted()
        {
            if (startupTimer != null)
            {
                throw new InvalidOperationException("Cannot Start because StartUpTracer has already been stared.");
            }
        }

        StringBuilderTraceListener CreateTraceListener()
        {
            return new StringBuilderTraceListener
            {
                TraceOutputOptions = TraceOptions.DateTime,
                Filter = new EventTypeFilter(SourceLevels.All)
            };
        }

    }
}