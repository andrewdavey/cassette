using Should;
using Xunit;
using System;

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

    public class Script_creation_contracts
    {
        [Fact]
        public void Absolute_path_required()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new Script("test.js", new byte[0], new string[0]);
            });
        }
    }
}
