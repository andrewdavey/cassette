using System.Diagnostics;

namespace Cassette
{
    static class Trace
    {
        public static readonly TraceSource Source = new TraceSource("Cassette");
    }
}

