using System;
using System.IO;
using System.Web;

namespace Cassette.Aspnet
{
    class PlaceholderReplacingResponseFilter : MemoryStream
    {
        public PlaceholderReplacingResponseFilter(HttpResponseBase response, IPlaceholderTracker placeholderTracker)
        {
            this.response = response;
            this.placeholderTracker = placeholderTracker;
            outputStream = response.Filter;
            bufferStream = new MemoryStream();
        }

        readonly Stream outputStream;
        readonly HttpResponseBase response;
        readonly IPlaceholderTracker placeholderTracker;
        readonly MemoryStream bufferStream;
        bool hasWrittenToOutputStream;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (HttpRuntime.UsingIntegratedPipeline && response.Headers["Content-Encoding"] != null)
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
            var outputBytes = response.Output.Encoding.GetBytes(output);
            if (outputBytes.Length > 0)
            {
                outputStream.Write(outputBytes, 0, outputBytes.Length);
            }
        }

        string GetOutputWithPlaceholdersReplaced()
        {
            var encoding = response.Output.Encoding;
            var originalOutput = encoding.GetString(bufferStream.ToArray());
            return placeholderTracker.ReplacePlaceholders(originalOutput);
        }
    }
}