using System.Diagnostics;
using System.Text;

namespace Cassette.Web
{
    class StringBuilderTraceListener : TraceListener
    {
        readonly StringBuilder builder = new StringBuilder();

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (eventType == TraceEventType.Information)
            {
                WriteLine(eventCache.DateTime.TimeOfDay + " - " + message);
            }
            else
            {
                base.TraceEvent(eventCache, source, eventType, id, message);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            var message = args == null ? format : string.Format(format, args);
            if (eventType == TraceEventType.Information)
            {
                WriteLine(eventCache.DateTime.TimeOfDay + " - " + message);
            }
            else
            {
                base.TraceEvent(eventCache, source, eventType, id, message);
            }
        }

        public override void Write(string message)
        {
            builder.Append(message);
        }

        public override void WriteLine(string message)
        {
            builder.AppendLine(message);
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
