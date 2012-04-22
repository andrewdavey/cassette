using System.Diagnostics;
using System.Text;

namespace Cassette.Aspnet
{
    class StringBuilderTraceListener : TraceListener
    {
        readonly StringBuilder builder = new StringBuilder();

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