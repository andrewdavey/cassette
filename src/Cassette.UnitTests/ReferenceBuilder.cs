using System;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ReferenceBuilder_Tests
    {
        public ReferenceBuilder_Tests()
        {
            moduleContainer = new Mock<IModuleContainer<ScriptModule>>();
            builder = new ReferenceBuilder<ScriptModule>(moduleContainer.Object);
        }

        readonly Mock<IModuleContainer<ScriptModule>> moduleContainer;
        readonly ReferenceBuilder<ScriptModule> builder;

        [Fact]
        public void WhenAddReferenceToModuleDirectory_ThenGetModulesReturnTheModule()
        {
            var module = new ScriptModule("test", Mock.Of<IFileSystem>());
            moduleContainer.Setup(c => c.FindModuleByPath("test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module }))
                           .Returns(new[] { module })
                           .Verifiable();
            builder.AddReference("test");

            var modules = builder.GetModules(null).ToArray();

            modules[0].ShouldBeSameAs(module);
            moduleContainer.Verify();
        }

        [Fact]
        public void WhenAddReferenceToModuleDirectoryWithLocation_ThenGetModulesThatLocationReturnTheModule()
        {
            var module = new ScriptModule("test", Mock.Of<IFileSystem>());
            module.Location = "body";
            moduleContainer.Setup(c => c.FindModuleByPath("test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module }))
                           .Returns(new[] { module })
                           .Verifiable();
            builder.AddReference("test");

            var modules = builder.GetModules("body").ToArray();

            modules[0].ShouldBeSameAs(module);
            moduleContainer.Verify();
        }

        [Fact]
        public void OnlyModulesMatchingLocationAreReturnedByGetModules()
        {
            var module1 = new ScriptModule("test1", Mock.Of<IFileSystem>());
            var module2 = new ScriptModule("test2", Mock.Of<IFileSystem>());
            module1.Location = "body";
            moduleContainer.Setup(c => c.FindModuleByPath("test1"))
                           .Returns(module1);
            moduleContainer.Setup(c => c.FindModuleByPath("test2"))
                           .Returns(module2);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module1 }))
                           .Returns(new[] { module1 });
            builder.AddReference("test1");
            builder.AddReference("test2");

            var modules = builder.GetModules("body").ToArray();
            modules.Length.ShouldEqual(1);
            modules[0].ShouldBeSameAs(module1);
        }

        [Fact]
        public void WhenAddReferenceToNonExistentModule_ThenThrowException()
        {
            moduleContainer.Setup(c => c.FindModuleByPath("test")).Returns((ScriptModule)null);

            Assert.Throws<ArgumentException>(delegate
            {
                builder.AddReference("test");
            });
        }

        [Fact]
        public void GivenModuleAReferencesModuleB_WhenAddReferenceToModuleA_ThenGetModulesReturnsBoth()
        {
            var moduleA = new ScriptModule("a", Mock.Of<IFileSystem>());
            var moduleB = new ScriptModule("b", Mock.Of<IFileSystem>());

            moduleContainer.Setup(c => c.FindModuleByPath("a"))
                           .Returns(moduleA);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { moduleA }))
                           .Returns(new[] { moduleB, moduleA });

            builder.AddReference("a");

            builder.GetModules(null).SequenceEqual(new[] { moduleB, moduleA }).ShouldBeTrue();
        }
    }
}
