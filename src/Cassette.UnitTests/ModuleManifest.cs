using Should;
using Xunit;

namespace Cassette
{
    public class ModuleManifest_comparisons
    {
        [Fact]
        public void Identical_ModuleManifests_have_no_differences()
        {
            var oldModuleManifest = new ModuleManifest(new[] 
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
            });

            var newModuleManifest = new ModuleManifest(new[] 
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
            });

            var differences = newModuleManifest.CompareTo(oldModuleManifest);
            differences.Length.ShouldEqual(0);
        }

        [Fact]
        public void Compare_ModuleManifest_with_changed_module_to_old_returns_Add_and_Delete_differences()
        {
            Module oldModule;
            var oldModuleManifest = new ModuleManifest(new[] 
            {
                oldModule = new Module(
                    @"scripts/module-a",
                    new Resource[]
                    {
                        new Resource(@"scripts/module-a/test.js", new byte[] { 1 }, new string[0])
                    }, 
                    new string[0],
                    null
                )
            });

            Module changedModule;
            var newModuleManifest = new ModuleManifest(new[] 
            {
                changedModule = new Module(
                    @"scripts/module-a",
                    new Resource[]
                    {
                        new Resource(@"scripts/module-a/test.js", new byte[] { 2 }, new string[0])
                    }, 
                    new string[0],
                    null
                )
            });

            var differences = newModuleManifest.CompareTo(oldModuleManifest);
            differences.Length.ShouldEqual(2);
            differences[0].Type.ShouldEqual(ModuleDifferenceType.Deleted);
            differences[0].Module.ShouldEqual(oldModule);
            differences[1].Type.ShouldEqual(ModuleDifferenceType.Added);
            differences[1].Module.ShouldEqual(changedModule);
        }

        [Fact]
        public void Compare_ModuleManifest_with_deleted_module_to_old_returns_difference()
        {
            Module module;
            var oldModuleManifest = new ModuleManifest(new[] 
            {
                module = new Module(
                    @"scripts/module-a",
                    new Resource[]
                    {
                        new Resource(@"scripts/module-a/test.js", new byte[0], new string[0])
                    }, 
                    new string[0],
                    null
                )
            });

            var newModuleManifest = new ModuleManifest(new Module[0]);

            var differences = newModuleManifest.CompareTo(oldModuleManifest);
            differences.Length.ShouldEqual(1);
            differences[0].Type.ShouldEqual(ModuleDifferenceType.Deleted);
            differences[0].Module.ShouldEqual(module);
        }

        [Fact]
        public void Compare_ModuleManifest_with_added_module_to_old_returns_difference()
        {
            var oldModuleManifest = new ModuleManifest(new Module[0]);

            Module module;
            var newModuleManifest = new ModuleManifest(new[] 
            {
                module = new Module(
                    @"scripts/module-a",
                    new Resource[0], 
                    new string[0],
                    null
                )
            });


            var differences = newModuleManifest.CompareTo(oldModuleManifest);
            differences.Length.ShouldEqual(1);
            differences[0].Type.ShouldEqual(ModuleDifferenceType.Added);
            differences[0].Module.ShouldEqual(module);
        }
    }
}
