using System;
using System.IO.IsolatedStorage;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleContainer_facts : IDisposable
    {
        readonly ModuleContainer moduleContainer;
        readonly IsolatedStorageFile storage;

        public ModuleContainer_facts()
        {
            storage = IsolatedStorageFile.GetUserStoreForDomain();
            moduleContainer = new ModuleContainer(new[] 
            {
                new Module(
                    @"scripts/module-a",
                    new Resource[]
                    {
                        new Resource(@"scripts/module-a/test.js", new byte[0], new string[0])
                    }, 
                    new string[0],
                    null
                ),
                new Module(@"scripts/module-b", new Resource[0], new string[0], null)
            }, storage, tw => null);
        }

        public void Dispose()
        {
            storage.Dispose();
        }

        [Fact]
        public void FindModuleContainingScript_with_known_path_returns_the_Module()
        {
            var module = moduleContainer.FindModuleContainingResource(@"scripts/module-a/test.js");
            module.Path.ShouldEqual(@"scripts/module-a");
        }

        [Fact]
        public void FindModuleContainingScript_with_known_path_different_case_returns_the_Module()
        {
            var module = moduleContainer.FindModuleContainingResource(@"SCRIPTS/module-a/test.js");
            module.Path.ShouldEqual(@"scripts/module-a");
        }

        [Fact]
        public void FindModuleContainingScript_with_unknown_path_returns_null()
        {
            var module = moduleContainer.FindModuleContainingResource(@"scripts/module-X/XXX.js");
            module.ShouldBeNull();
        }

        [Fact]
        public void Contains_known_path_returns_True()
        {
            moduleContainer.Contains(@"scripts/module-a").ShouldBeTrue();
        }

        [Fact]
        public void Contains_known_path_different_cased_returns_True()
        {
            moduleContainer.Contains(@"SCRIPTS/module-a").ShouldBeTrue();
        }

        [Fact]
        public void Contains_unknown_path_returns_False()
        {
            moduleContainer.Contains(@"scripts/module-X").ShouldBeFalse();
        }
    }

    public class ModuleContainer_differencing : IDisposable
    {
        readonly IsolatedStorageFile storage;

        public ModuleContainer_differencing()
        {
            storage = IsolatedStorageFile.GetUserStoreForDomain();
        }

        public void Dispose()
        {
            storage.Dispose();
        }

    }
}
