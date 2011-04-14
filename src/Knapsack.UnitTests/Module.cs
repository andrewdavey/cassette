using Should;
using Xunit;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Knapsack
{
    public class Given_a_new_Module
    {
        readonly Module module;
        readonly Script scriptA;
        readonly Script scriptB;

        public Given_a_new_Module()
        {
            scriptA = new Script(@"scripts\module-a\test-a.js", new byte[] { 1, 1, 1 }, new string[0]);
            scriptB = new Script(@"scripts\module-a\test-b.js", new byte[] { 2, 2, 2 }, new string[0]);
            module = new Module(
                @"scripts\module-a", // source path
                new[] { scriptA, scriptB }, // scripts
                new[] { @"scripts\module-b" } // references
            );
        }

        [Fact]
        public void has_Path_property()
        {
            module.Path.ShouldEqual(@"scripts\module-a");
        }

        [Fact]
        public void has_Scripts_property()
        {
            module.Scripts.ShouldEqual(new[] { scriptA, scriptB });
        }

        [Fact]
        public void has_References_property()
        {
            module.References.ShouldEqual(new[] { @"scripts\module-b" });
        }

        [Fact]
        public void Hash_property_is_sha1_of_script_hashes()
        {
            byte[] expectedHash;
            using (var sha1 = SHA1.Create())
            {
                expectedHash = sha1.ComputeHash(scriptA.Hash.Concat(scriptB.Hash).ToArray());
            }
            module.Hash.ShouldEqual(expectedHash);
        }
    }

}
