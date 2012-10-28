using System.IO;

namespace Cassette.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Returns a new stream containing the contents of the string, using UTF-8 encoding.
        /// The stream's Position property is set to zero.
        /// </summary>
        /// <param name="s">The string to convert into a stream.</param>
        /// <returns>A new stream.</returns>
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