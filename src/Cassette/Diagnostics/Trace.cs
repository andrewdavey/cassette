using System.Diagnostics;

namespace Cassette.Diagnostics
{
    public static class Trace
    {
        public static readonly TraceSource Source = new TraceSource("Cassette");
    }
}