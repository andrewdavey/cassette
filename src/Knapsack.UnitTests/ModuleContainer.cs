using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Should;

namespace Knapsack
{
    public class ModuleContainer_facts
    {
        readonly ModuleContainer moduleContainer;

        public ModuleContainer_facts()
        {
            moduleContainer = new ModuleContainer(new[] 
            {
                new Module(
                    @"c:/scripts/module-a",
                    new Script[]
                    {
                        new Script(@"c:/scripts/module-a/test.js", new byte[0], new string[0])
                    }, 
                    new string[0]
                ),
                new Module(@"c:/scripts/module-b", new Script[0], new string[0])
            });
        }

        [Fact]
        public void FindModuleContainingScript_with_known_path_returns_the_Module()
        {
            var module = moduleContainer.FindModuleContainingScript(@"c:/scripts/module-a/test.js");
            module.Path.ShouldEqual(@"c:/scripts/module-a");
        }

        [Fact]
        public void FindModuleContainingScript_with_unknown_path_returns_null()
        {
            var module = moduleContainer.FindModuleContainingScript(@"c:/scripts/module-X/XXX.js");
            module.ShouldBeNull();
        }

        [Fact]
        public void Contains_known_path_returns_True()
        {
            moduleContainer.Contains(@"c:/scripts/module-a").ShouldBeTrue();
        }

        [Fact]
        public void Contains_unknown_path_returns_False()
        {
            moduleContainer.Contains(@"c:/scripts/module-X").ShouldBeFalse();
        }
    }

    public class ModuleContainer_differencing
    {
        [Fact]
        public void Identical_ModuleContainers_have_no_differences()
        {
            var oldModuleContainer = new ModuleContainer(new[] 
            {
                new Module(
                    @"c:/scripts/module-a",
                    new Script[]
                    {
                        new Script(@"c:/scripts/module-a/test.js", new byte[0], new string[0])
                    }, 
                    new string[0]
                ),
                new Module(@"c:/scripts/module-b", new Script[0], new string[0])
            });

            var newModuleContainer = new ModuleContainer(new[] 
            {
                new Module(
                    @"c:/scripts/module-a",
                    new Script[]
                    {
                        new Script(@"c:/scripts/module-a/test.js", new byte[0], new string[0])
                    }, 
                    new string[0]
                ),
                new Module(@"c:/scripts/module-b", new Script[0], new string[0])
            });

            var differences = newModuleContainer.CompareTo(oldModuleContainer);
            differences.Length.ShouldEqual(0);
        }

        [Fact]
        public void Compare_ModuleContainer_with_changed_module_to_old_returns_difference()
        {
            var oldModuleContainer = new ModuleContainer(new[] 
            {
                new Module(
                    @"c:/scripts/module-a",
                    new Script[]
                    {
                        new Script(@"c:/scripts/module-a/test.js", new byte[] { 1 }, new string[0])
                    }, 
                    new string[0]
                )
            });

            Module changedModule;
            var newModuleContainer = new ModuleContainer(new[] 
            {
                changedModule = new Module(
                    @"c:/scripts/module-a",
                    new Script[]
                    {
                        new Script(@"c:/scripts/module-a/test.js", new byte[] { 2 }, new string[0])
                    }, 
                    new string[0]
                )
            });

            var differences = newModuleContainer.CompareTo(oldModuleContainer);
            differences.Length.ShouldEqual(1);
            differences[0].Type.ShouldEqual(ModuleDifferenceType.Changed);
            differences[0].Module.ShouldEqual(changedModule);
        }

        [Fact]
        public void Compare_ModuleContainer_with_deleted_module_to_old_returns_difference()
        {
            Module module;
            var oldModuleContainer = new ModuleContainer(new[] 
            {
                module = new Module(
                    @"c:/scripts/module-a",
                    new Script[]
                    {
                        new Script(@"c:/scripts/module-a/test.js", new byte[0], new string[0])
                    }, 
                    new string[0]
                )
            });

            var newModuleContainer = new ModuleContainer(new Module[0]);

            var differences = newModuleContainer.CompareTo(oldModuleContainer);
            differences.Length.ShouldEqual(1);
            differences[0].Type.ShouldEqual(ModuleDifferenceType.Deleted);
            differences[0].Module.ShouldEqual(module);
        }

        [Fact]
        public void Compare_ModuleContainer_with_added_module_to_old_returns_difference()
        {
            var oldModuleContainer = new ModuleContainer(new Module[0]);

            Module module;
            var newModuleContainer = new ModuleContainer(new[] 
            {
                module = new Module(
                    @"c:/scripts/module-a",
                    new Script[0], 
                    new string[0]
                )
            });


            var differences = newModuleContainer.CompareTo(oldModuleContainer);
            differences.Length.ShouldEqual(1);
            differences[0].Type.ShouldEqual(ModuleDifferenceType.Added);
            differences[0].Module.ShouldEqual(module);
        }
    }
}
