using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;

namespace Knapsack
{
    public class UnresolvedModule_with_scripts_having_only_internal_references
    {
        readonly UnresolvedModule unresolvedModule;
        readonly Module module;

        public UnresolvedModule_with_scripts_having_only_internal_references()
        {
            var scriptA = CreateScript("a", "c");
            var scriptB = CreateScript("b");
            var scriptC = CreateScript("c", "b");
            // Dependency chain: b <- c <- a 

            unresolvedModule = new UnresolvedModule(
                @"c:\scripts\module-a",
                new[] { scriptA, scriptB, scriptC }
            );

            module = unresolvedModule.Resolve(s => null);
        }

        [Fact]
        public void Resolve_creates_Module()
        {
            module.ShouldNotBeNull();
        }

        [Fact]
        public void Scripts_in_dependency_order()
        {
            var scriptFilenames = module.Scripts.Select(s => s.Path.Split('\\').Last()).ToArray();
            scriptFilenames.ShouldEqual(
                new[] { "b.js", "c.js", "a.js" }
            );
        }

        [Fact]
        public void Module_Path_is_set()
        {
            module.Path.ShouldEqual(@"c:\scripts\module-a");
        }

        Script CreateScript(string name, params string[] references)
        {
            return new Script(
                @"c:\scripts\module-a\" + name + ".js",
                new byte[0],
                references.Select(r => @"c:\scripts\module-a\" + r + ".js").ToArray()
            );
        }
    }

    public class Resolve_a_UnresolvedModule_with_script_having_an_external_reference
    {
        readonly UnresolvedModule unresolvedModule;
        readonly Module module;

        public Resolve_a_UnresolvedModule_with_script_having_an_external_reference()
        {
            var script = new Script(
                @"c:\scripts\module-a\test.js",
                new byte[0],
                new[] { @"c:\scripts\module-b\lib.js" }
            );

            unresolvedModule = new UnresolvedModule(
                @"c:\scripts\module-a",
                new[] { script }
            );

            module = unresolvedModule.Resolve(s => @"c:\scripts\module-b");
        }

        [Fact]
        public void Module_has_reference_to_module_b()
        {
            module.References.ShouldEqual(new[] { @"c:\scripts\module-b" });
        }

        [Fact]
        public void Module_Script_has_no_internal_references()
        {
            module.Scripts[0].References.ShouldBeEmpty();
        }
    }
}
