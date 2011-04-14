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
                @"c:\scripts\test.js",
                new byte[] { 1, 2, 3 },
                new string[] { @"c:\scripts\other.js" }
            );
        }

        [Fact]
        public void has_Path_property()
        {
            script.Path.ShouldEqual(@"c:\scripts\test.js");
        }

        [Fact]
        public void has_Hash_property()
        {
            script.Hash.ShouldEqual(new byte[] { 1,2,3 });
        }

        [Fact]
        public void has_References_property()
        {
            script.References.ShouldEqual(new[] { @"c:\scripts\other.js" });
        }
    }
}
