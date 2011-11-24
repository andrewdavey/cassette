#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.IO;
using System.Text;
using System.Web;

namespace Cassette.Web
{
    class PlaceholderReplacingResponseFilter : MemoryStream
    {
        public PlaceholderReplacingResponseFilter(HttpResponseBase response, IPlaceholderTracker placeholderTracker)
        {
            this.response = response;
            this.placeholderTracker = placeholderTracker;
            outputStream = response.Filter;
            htmlBuffer = new StringBuilder();
        }

        readonly Stream outputStream;
        readonly HttpResponseBase response;
        readonly IPlaceholderTracker placeholderTracker;
        readonly StringBuilder htmlBuffer;
        bool hasWrittenToOutputStream;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (HttpRuntime.UsingIntegratedPipeline && response.Headers["Content-Encoding"] != null)
            {
                throw new InvalidOperationException("Cannot rewrite page output when it has been compressed. Either set ICassetteApplication.IsHtmlRewritingEnabled to false in the Cassette configuration, or set <urlCompression dynamicCompressionBeforeCache=\"false\" /> in Web.config.");
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
            outputStream.Write(outputBytes, 0, outputBytes.Length);
        }
    }
}