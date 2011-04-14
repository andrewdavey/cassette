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
                    @"c:\scripts\module-a",
                    new Script[]
                    {
                        new Script(@"c:\scripts\module-a\test.js", new byte[0], new string[0])
                    }, 
                    new string[0]
                ),
                new Module(@"c:\scripts\module-b", new Script[0], new string[0])
            });
        }

        [Fact]
        public void FindModuleContainingScript_with_known_path_returns_the_Module()
        {
            var module = moduleContainer.FindModuleContainingScript(@"c:\scripts\module-a\test.js");
            module.Path.ShouldEqual(@"c:\scripts\module-a");
        }

        [Fact]
        public void FindModuleContainingScript_with_unknown_path_returns_null()
        {
            var module = moduleContainer.FindModuleContainingScript(@"c:\scripts\module-X\XXX.js");
            module.ShouldBeNull();
        }

        [Fact]
        public void Contains_known_path_returns_True()
        {
            moduleContainer.Contains(@"c:\scripts\module-a").ShouldBeTrue();
        }

        [Fact]
        public void Contains_unknown_path_returns_False()
        {
            moduleContainer.Contains(@"c:\scripts\module-X").ShouldBeFalse();
        }
    }
}
