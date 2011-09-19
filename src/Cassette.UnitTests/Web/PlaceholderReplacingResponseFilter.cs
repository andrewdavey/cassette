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

using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Cassette.UI;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class PlaceholderReplacingResponseFilter_Tests
    {
        readonly MemoryStream outputStream;
        readonly Mock<HttpResponseBase> response;
        readonly Mock<IPlaceholderTracker> placeholderTracker;
        readonly PlaceholderReplacingResponseFilter filter;

        public PlaceholderReplacingResponseFilter_Tests()
        {
            outputStream = new MemoryStream();
            response = new Mock<HttpResponseBase>();
            placeholderTracker = new Mock<IPlaceholderTracker>();

            placeholderTracker.Setup(h => h.ReplacePlaceholders(It.IsAny<string>()))
                      .Returns<string>(s => s);

            response.SetupGet(r => r.ContentType).Returns("text/html");
            response.SetupGet(r => r.Output.Encoding).Returns(Encoding.ASCII);
            response.SetupGet(r => r.Filter).Returns(outputStream);
            response.SetupGet(r => r.Headers).Returns(new NameValueCollection());
            filter = new PlaceholderReplacingResponseFilter(response.Object, placeholderTracker.Object);
        }

        [Fact]
        public void Write_sends_buffer_to_output_stream()
        {
            var html = "<html>\r\n";
            filter.Write(Encoding.ASCII.GetBytes(html), 0, html.Length);
            outputStream.Length.ShouldEqual(html.Length);
        }

        [Fact]
        public void Html_has_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("text/html");

            filter.Write(Encoding.ASCII.GetBytes("<html/>"), 0, 7);

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html/>"));
        }

        [Fact]
        public void XHtml_has_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("application/xhtml+xml");

            filter.Write(Encoding.ASCII.GetBytes("<html/>"), 0, 7);

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html/>"));
        }

        [Fact]
        public void Non_html_output_does_not_have_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("text/plain");

            filter.Write(Encoding.ASCII.GetBytes("test"), 0, 4);

            outputStream.GetBuffer().SequenceEqual(Encoding.ASCII.GetBytes("test"));
        }
    }
}

