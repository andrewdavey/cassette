using Should;
using Xunit;

namespace Cassette.Utilities
{
    public class ByteArrayExtensions_ToHexString_Tests
    {
        [Fact]
        public void Empty_array_returns_empty_string()
        {
            new byte[] { }.ToHexString().ShouldEqual("");
        }

        [Fact]
        public void _1_returns_01_as_string()
        {
            new byte[] { 1 }.ToHexString().ShouldEqual("01");
        }

        [Fact]
        public void _10_returns_0a_as_string()
        {
            new byte[] { 10 }.ToHexString().ShouldEqual("0a");
        }

        [Fact]
        public void _255_returns_ff_as_string()
        {
            new byte[] { 255 }.ToHexString().ShouldEqual("ff");
        }
    }

    public class ByteArrayExtensions_FromHexString_Tests
    {
        [Fact]
        public void _01_returns_1()
        {
            ByteArrayExtensions.FromHexString("01").ShouldEqual(new byte[] { 1 });
        }

        [Fact]
        public void _01ff_returns_1_255()
        {
            ByteArrayExtensions.FromHexString("01ff").ShouldEqual(new byte[] { 1, 255 });            
        }

        [Fact]
        public void _01FF_returns_1_255()
        {
            ByteArrayExtensions.FromHexString("01FF").ShouldEqual(new byte[] { 1, 255 });
        }
    }
}

