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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class PlaceholderReplacingResponseFilter_Tests : IDisposable
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

            response.SetupGet(r => r.Output.Encoding).Returns(Encoding.ASCII);
            response.SetupGet(r => r.Filter).Returns(outputStream);
            response.SetupGet(r => r.Headers).Returns(new NameValueCollection());
            filter = new PlaceholderReplacingResponseFilter(response.Object, placeholderTracker.Object);
        }

        [Fact]
        public void WhenWrite_ThenDataWrittenToOutputStream()
        {
            Write("<html></html>");

            Encoding.ASCII.GetString(outputStream.ToArray()).ShouldEqual("<html></html>");
        }

        [Fact]
        public void WhenWrite_ThenReplacePlaceholdersIsCalled()
        {
            Write("<html></html>");

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html></html>"));
        }

        [Fact]
        public void WhenTwoWriteCalls_ThenAllOutputIsWrittenToOutputStream()
        {
            Write("<html>start");
            Write("end</html>");

            Encoding.ASCII.GetString(outputStream.ToArray()).ShouldEqual("<html>startend</html>");
        }

        [Fact]
        public void WhenTwoWriteCalls_ThenReplacePlaceholdersIsCalledWithAllOutput()
        {
            Write("<html>start");
            Write("end</html>");

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html>startend</html>"));
        }

        [Fact]
        public void GivenWrittenContentDoesNotHaveCloseHtmlTag_WhenClose_ThenThrowInvalidOperationException()
        {
            Write("<html>start");

            Assert.Throws<InvalidOperationException>(() => filter.Close());
        }

        [Fact]
        public void WhenWriteContentAfterClosingHtmlTag_ThenItIsWrittenToOutputStream()
        {
            Write("<html></html>");
            Write("more");

            Encoding.ASCII.GetString(outputStream.ToArray()).ShouldEqual("<html></html>more");
        }

        void Write(string content)
        {
            filter.Write(Encoding.ASCII.GetBytes(content), 0, content.Length);
        }

        void IDisposable.Dispose()
        {
            filter.Dispose();
            outputStream.Dispose();
        }
    }
}