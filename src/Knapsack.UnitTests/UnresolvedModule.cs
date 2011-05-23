using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;
using System.IO;

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
                @"scripts/module-a",
                new[] { scriptA, scriptB, scriptC },
                null,
                false
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
            var scriptFilenames = module.Resources.Select(s => s.Path.Split('/').Last()).ToArray();
            scriptFilenames.ShouldEqual(
                new[] { "b.js", "c.js", "a.js" }
            );
        }

        [Fact]
        public void Module_Path_is_set()
        {
            module.Path.ShouldEqual(@"scripts/module-a");
        }

        UnresolvedResource CreateScript(string name, params string[] references)
        {
            return new UnresolvedResource(
                @"scripts/module-a/" + name + ".js",
                new byte[0],
                references.Select(r => r + ".js").ToArray()
            );
        }
    }

    public class Resolve_an_UnresolvedModule_with_scripts_in_subdirectory
    {
        readonly UnresolvedModule unresolvedModule;
        readonly Module module;

        public Resolve_an_UnresolvedModule_with_scripts_in_subdirectory()
        {
            var script1 = new UnresolvedResource(
                @"scripts/module-a/sub/test-1.js",
                new byte[0],
                new[] { @"test-2.js" }
            );
            var script2 = new UnresolvedResource(
                @"scripts/module-a/sub/test-2.js",
                new byte[0],
                new string[] { }
            );
            unresolvedModule = new UnresolvedModule(
                @"scripts/module-a",
                new[] { script1, script2 },
                null,
                false
            );

            module = unresolvedModule.Resolve(s => @"scripts/module-a");
        }

        [Fact]
        public void script_2_before_script_1()
        {
            module.Resources[0].Path.ShouldEqual(@"scripts/module-a/sub/test-2.js");
            module.Resources[1].Path.ShouldEqual(@"scripts/module-a/sub/test-1.js");
        }

        [Fact]
        public void script_1_has_resolved_reference_to_script_1()
        {
            module.Resources[1].References[0].ShouldEqual(@"scripts/module-a/sub/test-2.js");
        }

        [Fact]
        public void no_external_references()
        {
            module.References.ShouldBeEmpty();
        }
    }

    public class Resolve_a_UnresolvedModule_with_script_having_an_external_reference
    {
        readonly UnresolvedModule unresolvedModule;
        readonly Module module;

        public Resolve_a_UnresolvedModule_with_script_having_an_external_reference()
        {
            var script = new UnresolvedResource(
                @"scripts/module-a/test.js",
                new byte[0],
                new[] { @"scripts/module-b/lib.js" }
            );

            unresolvedModule = new UnresolvedModule(
                @"scripts/module-a",
                new[] { script },
                null,
                false
            );

            module = unresolvedModule.Resolve(s => @"scripts/module-b");
        }

        [Fact]
        public void Module_has_reference_to_module_b()
        {
            module.References.ShouldEqual(new[] { @"scripts/module-b" });
        }

        [Fact]
        public void Module_Script_has_no_internal_references()
        {
            module.Resources[0].References.ShouldBeEmpty();
        }
    }

    public class UnresolvedModule_with_fixed_resource_order
    {
        readonly UnresolvedModule unresolvedModule;
        readonly Module module;

        public UnresolvedModule_with_fixed_resource_order()
        {
            // Create scripts with some dependencies declared.
            var scriptA = CreateScript("a", "c");
            var scriptB = CreateScript("b");
            var scriptC = CreateScript("c", "b");
            // Declared dependency chain: b <- c <- a 

            unresolvedModule = new UnresolvedModule(
                @"scripts/module-a",
                new[] { scriptA, scriptB, scriptC },
                null,
                isResourceOrderFixed: true
            );

            module = unresolvedModule.Resolve(s => null);
        }

        [Fact]
        public void Original_resource_ordering_is_maintained()
        {
            module.Resources
                .Select(r => r.Path)
                .SequenceEqual(new[] { "scripts/module-a/a.js", "scripts/module-a/b.js", "scripts/module-a/c.js" })
                .ShouldBeTrue();
        }

        UnresolvedResource CreateScript(string name, params string[] references)
        {
            return new UnresolvedResource(
                @"scripts/module-a/" + name + ".js",
                new byte[0],
                references.Select(r => r + ".js").ToArray()
            );
        }
    }

    public class UnresolvedModule_ResolveAll
    {
        [Fact]
        public void Returns_resolved_modules()
        {
            var moduleA = new UnresolvedModule("module-a", new[] { CreateScript("module-a", "foo") }, null, false);
            var moduleB = new UnresolvedModule("module-b", new[] { CreateScript("module-b", "bar", "../module-a/foo") }, null, false);
            
            var modules = UnresolvedModule.ResolveAll(new[] { moduleA, moduleB }).ToArray();

            modules[0].Path.ShouldEqual("module-a");
            modules[1].Path.ShouldEqual("module-b");
            modules[1].References[0].ShouldEqual("module-a");
        }

        [Fact]
        public void Throws_useful_exception_when_resource_not_found()
        {
            var moduleA = new UnresolvedModule("module-a", new[] { CreateScript("module-a", "foo") }, null, false);
            var moduleB = new UnresolvedModule("module-b", new[] { CreateScript("module-b", "bar", "../module-a/missing") }, null, false);

            var exception = Assert.Throws<FileNotFoundException>(delegate
            {
                UnresolvedModule.ResolveAll(new[] { moduleA, moduleB }).ToArray();
            });
            exception.FileName.ShouldEqual("module-a/missing.js");
            exception.Message.ShouldEqual(
                "The file \"module-a/missing.js\" is referenced by \"module-b/bar.js\", but cannot be found. " +
                "Either add \"module-a/missing.js\" or change the reference(s) to a file that exists.");
        }

        UnresolvedResource CreateScript(string module, string name, params string[] references)
        {
            return new UnresolvedResource(
                module + "/" + name + ".js",
                new byte[0],
                references.Select(r => r + ".js").ToArray()
            );
        }
    }
}
