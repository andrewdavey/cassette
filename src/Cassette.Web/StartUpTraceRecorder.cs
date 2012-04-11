using System;
using System.Diagnostics;

namespace Cassette.Web
{
    /// <summary>
    /// Collects all of Cassette's trace output during start-up.
    /// </summary>
    class StartUpTraceRecorder : IDisposable
    {
        readonly TraceListener startupTraceListener;

        public StartUpTraceRecorder()
        {
            startupTraceListener = CreateTraceListener();
            Trace.Source.Listeners.Add(startupTraceListener);
        }

        public void Dispose()
        {
            Trace.Source.Listeners.Remove(startupTraceListener);
        }

        public string TraceOutput
        {
            get
            {
                Trace.Source.Flush();
                startupTraceListener.Flush();
                return startupTraceListener.ToString();
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