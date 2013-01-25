using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Cassette.Aspnet
{
    class PlaceholderReplacingResponseFilter : MemoryStream
    {
        public PlaceholderReplacingResponseFilter(Stream outputStream, Encoding outputEncoding, NameValueCollection responseHeaders, IPlaceholderTracker placeholderTracker)
        {
            this.outputStream = outputStream;
            this.outputEncoding = outputEncoding;
            this.responseHeaders = responseHeaders;
            this.placeholderTracker = placeholderTracker;
            bufferStream = new MemoryStream();
        }

        readonly Stream outputStream;
        readonly Encoding outputEncoding;
        readonly NameValueCollection responseHeaders;
        readonly IPlaceholderTracker placeholderTracker;
        readonly MemoryStream bufferStream;
        bool hasWrittenToOutputStream;

        public override void Write(byte[] buffer, int offset, int count)
        {
            bufferStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            if (!hasWrittenToOutputStream)
            {
                WriteBufferedOutput();
                hasWrittenToOutputStream = true;
            }

            base.Close();
        }

        void WriteBufferedOutput()
        {
            var contentEncoding = responseHeaders["Content-Encoding"] ?? "";
            if (contentEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                WriteCompressedOutput((stream, mode) => new GZipStream(stream, mode));
            }
            else if (contentEncoding.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                WriteCompressedOutput((stream, mode) => new DeflateStream(stream, mode));
            }
            else if (contentEncoding != "")
            {
                throw new Exception("Cannot process output with content encoding \"" + contentEncoding + "\".");
            }
            else
            {
                var output = GetOutputWithPlaceholdersReplaced();
                var outputBytes = outputEncoding.GetBytes(output);
                if (outputBytes.Length > 0)
                {
                    outputStream.Write(outputBytes, 0, outputBytes.Length);
                }
            }
        }

        void WriteCompressedOutput(Func<Stream, CompressionMode, Stream> createCompressionStream)
        {
            bufferStream.Position = 0;
            using (var decompressor = createCompressionStream(bufferStream, CompressionMode.Decompress))
            using (var expanded = new MemoryStream())
            {
                decompressor.CopyTo(expanded);
                var originalOutput = outputEncoding.GetString(expanded.ToArray());
                var replacedOutput = placeholderTracker.ReplacePlaceholders(originalOutput);
                using (var toCompress = new MemoryStream(outputEncoding.GetBytes(replacedOutput)))
                using (var compressor = createCompressionStream(outputStream, CompressionMode.Compress))
                {
                    toCompress.CopyTo(compressor);
                }
            }
        }

        string GetOutputWithPlaceholdersReplaced()
        {
            var originalOutput = outputEncoding.GetString(bufferStream.ToArray());
            return placeholderTracker.ReplacePlaceholders(originalOutput);
        }
    }
}