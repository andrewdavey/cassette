using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Text;

#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Aspnet
{
    class PlaceholderReplacingResponseFilter : MemoryStream
    {
        public PlaceholderReplacingResponseFilter(Stream outputStream, Encoding outputEncoding, NameValueCollection responseHeaders, IPlaceholderTracker placeholderTracker)
        {
            var gzipStream = outputStream as GZipStream;
            var deflateStream = outputStream as DeflateStream;

            // get the base stream if the outout stream is a compressor already.
            // this can happen, when the top application is setting a compressor as the response filter
            if (gzipStream != null)
            {
                this.outputStream = gzipStream.BaseStream;
                decompressFirst = false;
            }
            else if (deflateStream != null)
            {
                this.outputStream = deflateStream.BaseStream;
                decompressFirst = false;
            }
            else
            {
                this.outputStream = outputStream;
                decompressFirst = true;
            }

            this.outputEncoding = outputEncoding;
            this.responseHeaders = responseHeaders;
            this.placeholderTracker = placeholderTracker;
            this.bufferStream = new MemoryStream();
        }

        readonly Stream outputStream;
        readonly Encoding outputEncoding;
        readonly NameValueCollection responseHeaders;
        readonly IPlaceholderTracker placeholderTracker;
        readonly bool decompressFirst;

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
            // The buffered stream may be compressed already.
            // Detect this from the Content-Encoding header.
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
                throw new Exception("Cannot process response with content encoding \"" + contentEncoding + "\".");
            }
            else
            {
                WriteUncompressedOutput();
            }
        }

        void WriteCompressedOutput(Func<Stream, CompressionMode, Stream> createCompressionStream)
        {
            // Decompress the buffered stream into an in memory stream.
            bufferStream.Position = 0;
            string replacedOutput;

            if (decompressFirst)
            {
                using (var decompressor = createCompressionStream(bufferStream, CompressionMode.Decompress))
                using (var expandedOriginalStream = new MemoryStream())
                {
                    // Replace placeholders in the original content
                    decompressor.CopyTo(expandedOriginalStream);
                    replacedOutput = GetOutputWithPlaceholdersReplaced(expandedOriginalStream);
                }
            }
            else
            {
                // Replace placeholders in the original content
                replacedOutput = GetOutputWithPlaceholdersReplaced(bufferStream);
            }

            // Compress this updated content,
            // sending it to the output stream.
            using (var toCompress = new MemoryStream(outputEncoding.GetBytes(replacedOutput)))
            using (var compressor = createCompressionStream(outputStream, CompressionMode.Compress))
            {
                toCompress.CopyTo(compressor);
            }
        }

        void WriteUncompressedOutput()
        {
            var output = GetOutputWithPlaceholdersReplaced(bufferStream);
            var outputBytes = outputEncoding.GetBytes(output);
            if (outputBytes.Length > 0)
            {
                outputStream.Write(outputBytes, 0, outputBytes.Length);
				outputStream.Flush();
            }
        }

        string GetOutputWithPlaceholdersReplaced(MemoryStream originalStream)
        {
            var originalOutput = outputEncoding.GetString(originalStream.ToArray());
            return placeholderTracker.ReplacePlaceholders(originalOutput);
        }
    }
}
