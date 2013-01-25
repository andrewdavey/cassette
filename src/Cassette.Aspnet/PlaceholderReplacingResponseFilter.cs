using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

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
            if (HttpRuntime.UsingIntegratedPipeline && responseHeaders["Content-Encoding"] != null)
            {
                throw new InvalidOperationException("Cannot rewrite page output when it has been compressed. Either set <cassette rewriteHtml=\"false\" /> to disable rewriting or set <urlCompression dynamicCompressionBeforeCache=\"false\" /> in Web.config.");
            }

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
            var output = GetOutputWithPlaceholdersReplaced();
            var outputBytes = outputEncoding.GetBytes(output);
            if (outputBytes.Length > 0)
            {
                outputStream.Write(outputBytes, 0, outputBytes.Length);
            }
        }

        string GetOutputWithPlaceholdersReplaced()
        {
            var originalOutput = outputEncoding.GetString(bufferStream.ToArray());
            return placeholderTracker.ReplacePlaceholders(originalOutput);
        }
    }
}