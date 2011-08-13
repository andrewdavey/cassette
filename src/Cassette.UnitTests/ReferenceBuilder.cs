using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
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
            moduleFactory = new Mock<IModuleFactory<ScriptModule>>();
            builder = new ReferenceBuilder<ScriptModule>(moduleContainer.Object, moduleFactory.Object);
        }

        readonly ReferenceBuilder<ScriptModule> builder;
        readonly Mock<IModuleContainer<ScriptModule>> moduleContainer;
        readonly Mock<IModuleFactory<ScriptModule>> moduleFactory;

        [Fact]
        public void WhenAddReferenceToModuleDirectory_ThenGetModulesReturnTheModule()
        {
            var module = new ScriptModule("test");
            moduleContainer.Setup(c => c.FindModuleByPath("test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module }))
                           .Returns(new[] { module })
                           .Verifiable();
            builder.AddReference("test", null);

            var modules = builder.GetModules(null).ToArray();

            modules[0].ShouldBeSameAs(module);
            moduleContainer.Verify();
        }

        [Fact]
        public void WhenAddReferenceToModuleDirectoryWithLocation_ThenGetModulesThatLocationReturnTheModule()
        {
            var module = new ScriptModule("test");
            module.Location = "body";
            moduleContainer.Setup(c => c.FindModuleByPath("test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module }))
                           .Returns(new[] { module })
                           .Verifiable();
            builder.AddReference("test", null);

            var modules = builder.GetModules("body").ToArray();

            modules[0].ShouldBeSameAs(module);
            moduleContainer.Verify();
        }

        [Fact]
        public void OnlyModulesMatchingLocationAreReturnedByGetModules()
        {
            var module1 = new ScriptModule("test1");
            var module2 = new ScriptModule("test2");
            module1.Location = "body";
            moduleContainer.Setup(c => c.FindModuleByPath("test1"))
                           .Returns(module1);
            moduleContainer.Setup(c => c.FindModuleByPath("test2"))
                           .Returns(module2);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module1 }))
                           .Returns(new[] { module1 });
            builder.AddReference("test1", null);
            builder.AddReference("test2", null);

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
                builder.AddReference("test", null);
            });
        }

        [Fact]
        public void GivenModuleAReferencesModuleB_WhenAddReferenceToModuleA_ThenGetModulesReturnsBoth()
        {
            var moduleA = new ScriptModule("a");
            var moduleB = new ScriptModule("b");

            moduleContainer.Setup(c => c.FindModuleByPath("a"))
                           .Returns(moduleA);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { moduleA }))
                           .Returns(new[] { moduleB, moduleA });

            builder.AddReference("a", null);

            builder.GetModules(null).SequenceEqual(new[] { moduleB, moduleA }).ShouldBeTrue();
        }

        [Fact]
        public void WhenAddReferenceToUrl_ThenGetModulesReturnsAnExternalModule()
        {
            moduleFactory.Setup(f => f.CreateExternalModule("http://test.com/test.js"))
                         .Returns(new ExternalScriptModule("http://test.com/test.js"));
            moduleContainer.Setup(c => c.AddDependenciesAndSort(It.IsAny<IEnumerable<ScriptModule>>()))
                           .Returns<IEnumerable<ScriptModule>>(all => all);

            builder.AddReference("http://test.com/test.js", null);

            var module = builder.GetModules(null).First();
            module.ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void WhenAddReferenceToHttpsUrl_ThenGetModulesReturnsAnExternalModule()
        {
            moduleFactory.Setup(f => f.CreateExternalModule("https://test.com/test.js"))
                         .Returns(new ExternalScriptModule("https://test.com/test.js"));
            moduleContainer.Setup(c => c.AddDependenciesAndSort(It.IsAny<IEnumerable<ScriptModule>>()))
                           .Returns<IEnumerable<ScriptModule>>(all => all);

            builder.AddReference("https://test.com/test.js", null);

            var module = builder.GetModules(null).First();
            module.ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void WhenAddReferenceToProtocolRelativeUrl_ThenGetModulesReturnsAnExternalModule()
        {
            moduleFactory.Setup(f => f.CreateExternalModule("//test.com/test.js"))
                         .Returns(new ExternalScriptModule("//test.com/test.js"));
            moduleContainer.Setup(c => c.AddDependenciesAndSort(It.IsAny<IEnumerable<ScriptModule>>()))
                           .Returns<IEnumerable<ScriptModule>>(all => all);

            builder.AddReference("//test.com/test.js", null);

            var module = builder.GetModules(null).First();
            module.ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void WhenAddReferenceWithLocation_ThenGetModulesForThatLocationReturnsTheModule()
        {
            var module = new ScriptModule("test");
            moduleContainer.Setup(c => c.FindModuleByPath("test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.AddDependenciesAndSort(new[] { module }))
                           .Returns(new[] { module });
            builder.AddReference("test", "body");

            builder.GetModules("body").SequenceEqual(new[] { module}).ShouldBeTrue();
        }
    }
}
