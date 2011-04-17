using Should;
using Xunit;

namespace Knapsack
{
    public class Given_a_new_Script
    {
        readonly Script script;

        public Given_a_new_Script()
        {
            script = new Script(
                @"scripts/test.js",
                new byte[] { 1, 2, 3 },
                new string[] { @"scripts/other.js" }
            );
        }

        [Fact]
        public void has_Path_property()
        {
            script.Path.ShouldEqual(@"scripts/test.js");
        }

        [Fact]
        public void has_Hash_property()
        {
            script.Hash.ShouldEqual(new byte[] { 1,2,3 });
        }

        [Fact]
        public void has_References_property()
        {
            script.References.ShouldEqual(new[] { @"scripts/other.js" });
        }
    }

}
