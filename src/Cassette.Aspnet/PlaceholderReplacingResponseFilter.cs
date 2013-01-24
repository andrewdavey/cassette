using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace Cassette.Aspnet
{
    class PlaceholderReplacingResponseFilter : MemoryStream
    {
        public PlaceholderReplacingResponseFilter(CassetteSettings settings, HttpResponseBase response, HttpRequestBase request, IPlaceholderTracker placeholderTracker)
        {
            this.settings = settings;
            this.response = response;
            this.request = request;
            this.placeholderTracker = placeholderTracker;
            outputStream = response.Filter;
            htmlBuffer = new StringBuilder();
        }

        readonly CassetteSettings settings;
        readonly Stream outputStream;
        readonly HttpResponseBase response;
        readonly HttpRequestBase request;
        readonly IPlaceholderTracker placeholderTracker;
        readonly StringBuilder htmlBuffer;
        bool hasWrittenToOutputStream;

        

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (HttpRuntime.UsingIntegratedPipeline && response.Headers["Content-Encoding"] != null)
            {
                throw new InvalidOperationException("Cannot rewrite page output when it has been compressed. Either set <cassette rewriteHtml=\"false\" /> to disable rewriting or set <urlCompression dynamicCompressionBeforeCache=\"false\" /> in Web.config.");
            }

            BufferOutput(buffer, offset, count);
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

        void BufferOutput(byte[] buffer, int offset, int count)
        {
            var encoding = response.Output.Encoding;
            var html = encoding.GetString(buffer, offset, count);
            htmlBuffer.Append(html);
        }

        void WriteBufferedOutput()
        {
            var encoding = response.Output.Encoding;
            var output = placeholderTracker.ReplacePlaceholders(htmlBuffer.ToString());
            var outputBytes = encoding.GetBytes(output);
            if (outputBytes.Length > 0)
            {
                var acceptEncodingRequest = request.Headers["Accept-Encoding"];
                if (settings.IsCompressionEnabled && !String.IsNullOrEmpty(acceptEncodingRequest) && acceptEncodingRequest.Contains("gzip"))
                {
                    response.AppendHeader("Content-Encoding", "gzip");
                    response.Cache.VaryByHeaders["Accept-Encoding"] = true;
                    var compressedStream = new GZipStream(outputStream, CompressionMode.Compress);
                    compressedStream.Write(outputBytes, 0, outputBytes.Length);
                    compressedStream.Close();
                }
                else
                {
                    outputStream.Write(outputBytes, 0, outputBytes.Length);
                }
            }
        }
    }
}