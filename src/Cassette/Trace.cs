using System.Diagnostics;

namespace Cassette
{
    public static class Trace
    {
        public static readonly TraceSource Source = new TraceSource("Cassette");
    }
}
