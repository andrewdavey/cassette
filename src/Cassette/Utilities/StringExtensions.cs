using System.IO;

namespace Cassette.Utilities
{
    public static class StringExtensions
    {
        public static Stream AsStream(this string s)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(s);
            writer.Flush();
            source.Position = 0;
            return source;
        }
    }
}
