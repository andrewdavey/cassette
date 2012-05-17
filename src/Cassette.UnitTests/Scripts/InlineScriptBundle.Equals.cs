using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class InlineScriptBundleEqualityTests
    {
        [Fact]
        public void TwoInstancesAreNotEqual() // refs #262
        {
            var bundle1 = new InlineScriptBundle("var x = 1;");
            var bundle2 = new InlineScriptBundle("var x = 1;");

            bundle1.Equals(bundle2).ShouldBeFalse();
        }

        [Fact]
        public void TheSameInstanceIsEqual()
        {
            var bundle1 = new InlineScriptBundle("var x = 1;");

            bundle1.Equals(bundle1).ShouldBeTrue();
        }
    }
}