using System.IO;

namespace Cassette.IntegrationTests
{
    static class StreamExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
