using System.IO;
using System.Web;

namespace Cassette.Web
{
    class PlaceholderReplacingResponseFilter : Stream
    {
        public PlaceholderReplacingResponseFilter(HttpResponseBase response, IPlaceholderTracker placeholderTracker)
        {
            this.response = response;
            this.placeholderTracker = placeholderTracker;
            this.outputStream = response.Filter;
            buffer = new MemoryStream();
        }

        readonly Stream outputStream;
        readonly HttpResponseBase response;
        readonly IPlaceholderTracker placeholderTracker;
        readonly MemoryStream buffer;
        long nextFlushStartPosition;

        public override void Flush()
        {
            buffer.Position = nextFlushStartPosition;
            // Next time we flush, skip over the bytes we've already sent.
            nextFlushStartPosition = buffer.Length;

            if (IsHtmlResponse)
            {
                SendHtmlResponse();
            }
            else
            {
                // Don't interfere with non-html output.
                buffer.CopyTo(outputStream);
            }

            outputStream.Flush();
        }

        void SendHtmlResponse()
        {
            var encoding = response.Output.Encoding;

            // Spin through the buffer looking for the placeholders.
            // Replace with the actual HTML elements.
            using (var reader = new StreamReader(buffer, encoding))
            {
                var writer = new StreamWriter(outputStream, encoding);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = placeholderTracker.ReplacePlaceholders(line);
                    writer.WriteLine(line);
                }
                writer.Flush();
                // Do not dispose the writer because that will also dispose the outputStream!
            }
        }

        bool IsHtmlResponse
        {
            get
            {
                return response.ContentType == "text/html" ||
                       response.ContentType == "application/xhtml+xml";
            }
        }

        // The rest of the Stream members are delegated to the buffer stream.

        public override bool CanRead
        {
            get { return buffer.CanRead; }
        }

        public override bool CanSeek
        {
            get { return buffer.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return buffer.CanWrite; }
        }

        public override long Length
        {
            get { return buffer.Length; }
        }

        public override long Position
        {
            get
            {
                return buffer.Position;
            }
            set
            {
                buffer.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.buffer.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return buffer.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            buffer.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.buffer.Write(buffer, offset, count);
        }
    }
}
