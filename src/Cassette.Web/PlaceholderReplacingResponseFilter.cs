using System;
using System.IO;
using System.Web;
using Cassette.UI;

namespace Cassette.Web
{
    public class PlaceholderReplacingResponseFilter : MemoryStream
    {
        public PlaceholderReplacingResponseFilter(HttpResponseBase response, IPlaceholderTracker placeholderTracker)
        {
            this.response = response;
            this.placeholderTracker = placeholderTracker;
            outputStream = response.Filter;
        }

        readonly Stream outputStream;
        readonly HttpResponseBase response;
        readonly IPlaceholderTracker placeholderTracker;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (response.Headers["Content-Encoding"] != null)
            {
                throw new InvalidOperationException("Cannot rewrite page output when it has been compressed. Either set ICassetteApplication.HtmlRewritingEnabled to false in the Cassette configuration, or set <urlCompression dynamicCompressionBeforeCache=\"false\" /> in Web.config.");
            }

            if (IsHtmlResponse)
            {
                buffer = ReplacePlaceholders(buffer, offset, count);
                outputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                outputStream.Write(buffer, offset, count);
            }
        }

        byte[] ReplacePlaceholders(byte[] buffer, int offset, int count)
        {
            var encoding = response.Output.Encoding;
            var html = encoding.GetString(buffer, offset, count);
            html = placeholderTracker.ReplacePlaceholders(html);
            return encoding.GetBytes(html);
        }

        bool IsHtmlResponse
        {
            get
            {
                return response.ContentType == "text/html" ||
                       response.ContentType == "application/xhtml+xml";
            }
        }
    }
}
