using System.Linq;
using System.Security.Cryptography;
using Should;
using Xunit;
using System;

namespace Knapsack
{
    public class Given_a_new_Module
    {
        readonly Module module;
        readonly Resource scriptA;
        readonly Resource scriptB;

        public Given_a_new_Module()
        {
            scriptA = new Resource(@"scripts/module-a/test-a.js", new byte[] { 1, 1, 1 }, new string[0]);
            scriptB = new Resource(@"scripts/module-a/test-b.js", new byte[] { 2, 2, 2 }, new string[0]);
            module = new Module(
                @"scripts/module-a", // source path
                new[] { scriptA, scriptB }, // scripts
                new[] { @"scripts/module-b" } // references
            );
        }

        [Fact]
        public void has_Path_property()
        {
            module.Path.ShouldEqual(@"scripts/module-a");
        }

        [Fact]
        public void has_Scripts_property()
        {
            module.Resources.ShouldEqual(new[] { scriptA, scriptB });
        }

        [Fact]
        public void has_References_property()
        {
            module.References.ShouldEqual(new[] { @"scripts/module-b" });
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

    public class Module_equality
    {
        [Fact]
        public void Modules_equal_when_same_path_and_hash()
        {
            var module1 = new Module("module-a", new[] { new Resource(@"module-a/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0]);
            var module2 = new Module("module-a", new[] { new Resource(@"module-a/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0]);
            Assert.Equal(module1, module2);
        }

        [Fact]
        public void Modules_not_equal_when_different_path()
        {
            var module1 = new Module("module-a", new[] { new Resource(@"module-a/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0]);
            var module2 = new Module("module-XX", new[] { new Resource(@"module-XX/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0]);
            Assert.NotEqual(module1, module2);
        }

        [Fact]
        public void Modules_not_equal_when_different_hash()
        {
            var module1 = new Module("module-a", new[] { new Resource(@"module-a/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0]);
            var module2 = new Module("module-a", new[] { new Resource(@"module-a/test.js", new byte[] { 9, 9, 9 }, new string[0]) }, new string[0]);
            Assert.NotEqual(module1, module2);
        }
    }

    public class Module_constructor
    {
        [Fact]
        public void Cannot_create_Module_with_script_having_path_not_in_module()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new Module("module-a", new[] { new Resource("module-b/test.js", new byte[0], new string[0]) }, new string[0]);
            });
        }
    }
}
