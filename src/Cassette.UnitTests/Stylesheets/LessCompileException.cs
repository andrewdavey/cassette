using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class LessCompileException_Tests
    {
        [Fact]
        public void LessCompileExceptionConstructorAcceptsMessage()
        {
            new LessCompileException("test").Message.ShouldEqual("test");
        }
    }
}

