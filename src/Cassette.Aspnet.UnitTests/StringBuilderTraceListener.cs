using Should;
using Xunit;
using System;

namespace Cassette.Aspnet
{
    public class StringBuilderTraceListener_Tests
    {
        readonly StringBuilderTraceListener listener;

        public StringBuilderTraceListener_Tests()
        {
            listener = new StringBuilderTraceListener();
        }

        [Fact]
        public void WhenWriteMessage_ThenToStringReturnsMessage()
        {
            listener.Write("test");
            listener.ToString().ShouldEqual("test");
        }

        [Fact]
        public void WhenWriteLineMessage_ThenToStringReturnsMessage()
        {
            listener.WriteLine("test");
            listener.ToString().ShouldEqual("test" + Environment.NewLine);
        }
    }
}