using Should;
using Xunit;

namespace Knapsack.Utilities
{
    public class ByteArrayExtensions_ToHexString_tests
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
}
