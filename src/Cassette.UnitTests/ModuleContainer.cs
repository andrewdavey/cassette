using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleContainer_Tests
    {
        void SetupAsset(string filename, Mock<IAsset> asset)
        {
            asset.Setup(a => a.SourceFilename).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReference_ThenConstructorThrowsAssetReferenceException()
        {
            var module = new Module("~/module-1");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~\\fail\\fail.js", asset.Object, 0, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module });
            });
            exception.Message.ShouldEqual("Reference error in \"module-1\\a.js\". Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReferenceHavingLineNumber_ThenConstructorThrowsAssetReferenceException()
        {
            var module = new Module("~/module-1");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/module-1/a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~\\fail\\fail.js", asset.Object, 42, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module });
            });
            exception.Message.ShouldEqual("Reference error in \"~/module-1/a.js\", line 42. Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void FindModuleContainingPathOfModuleReturnsTheModule()
        {
            var expectedModule = new Module("~/test");
            var container = new ModuleContainer<Module>(new[] {
                expectedModule
            });
            var actualModule = container.FindModuleContainingPath("~/test");
            actualModule.ShouldBeSameAs(expectedModule);
        }

        [Fact]
        public void FindModuleContainingPathWithWrongPathReturnsNull()
        {
            var container = new ModuleContainer<Module>(new[] {
                new Module("~/test")
            });
            var actualModule = container.FindModuleContainingPath("~/WRONG");
            actualModule.ShouldBeNull();
        }

        [Fact]
        public void FindModuleContainingPathOfAssetReturnsTheModule()
        {
            var expectedModule = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/test.js");
            expectedModule.Assets.Add(asset.Object);
            var container = new ModuleContainer<Module>(new[] {
                expectedModule
            });
            var actualModule = container.FindModuleContainingPath("~/test/test.js");
            actualModule.ShouldBeSameAs(expectedModule);
        }

        [Fact]
        public void GivenModule1ReferencesModule2_ThenConcatDependenciesModule1ReturnsModule2AndModule1()
        {
            var module1 = new Module("~/module-1");
            var asset1 = new Mock<IAsset>();
            SetupAsset("~/module-1/a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/module-2/b.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module1.Assets.Add(asset1.Object);

            var module2 = new Module("~/module-2");
            var asset2 = new Mock<IAsset>();
            SetupAsset("~/module-2/b.js", asset2);
            module2.Assets.Add(asset2.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2 });

            container.ConcatDependencies(module1)
                .SequenceEqual(new[] { module1, module2 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenModule1ReferencesModule2WhichReferencesModule3_ThenConcatDependenciesModule1ReturnsModule3AndModule2AndModule1()
        {
            var module1 = new Module("~/module-1");
            var asset1 = new Mock<IAsset>();
            SetupAsset("~/module-1/a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/module-2/b.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module1.Assets.Add(asset1.Object);

            var module2 = new Module("~/module-2");
            var asset2 = new Mock<IAsset>();
            SetupAsset("~/module-2/b.js", asset2);
            asset2.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/module-3/c.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module2.Assets.Add(asset2.Object);

            var module3 = new Module("~/module-3");
            var asset3 = new Mock<IAsset>();
            SetupAsset("~/module-3/c.js", asset3);
            module3.Assets.Add(asset3.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2, module3 });

            var actual = new HashSet<Module>(container.ConcatDependencies(module1));
            var expected = new[] { module1, module2, module3 };
            actual.SetEquals(expected).ShouldBeTrue();
        }

        [Fact]
        public void GivenDiamondReferencing_ThenConcatDependenciesReturnsEachReferencedModuleOnlyOnceInDependencyOrder()
        {
            var module1 = new Module("~/module-1");
            var asset1 = new Mock<IAsset>();
            SetupAsset("~/module-1/a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { 
                      new AssetReference("~/module-2/b.js", asset1.Object, 1, AssetReferenceType.DifferentModule),
                      new AssetReference("~/module-3/c.js", asset1.Object, 1, AssetReferenceType.DifferentModule)
                  });
            module1.Assets.Add(asset1.Object);

            var module2 = new Module("~/module-2");
            var asset2 = new Mock<IAsset>();
            SetupAsset("~/module-2/b.js", asset2);
            asset2.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/module-4/d.js", asset2.Object, 1, AssetReferenceType.DifferentModule) });
            module2.Assets.Add(asset2.Object);

            var module3 = new Module("~/module-3");
            var asset3 = new Mock<IAsset>();
            SetupAsset("~/module-3/c.js", asset3);
            asset3.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/module-4/d.js", asset3.Object, 1, AssetReferenceType.DifferentModule) });
            module3.Assets.Add(asset3.Object);

            var module4 = new Module("~/module-4");
            var asset4 = new Mock<IAsset>();
            SetupAsset("~/module-4/d.js", asset4);
            module4.Assets.Add(asset4.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2, module3, module4 });

            container.SortModules(container.ConcatDependencies(module1))
                .SequenceEqual(new[] { module4, module2, module3, module1 }).ShouldBeTrue();
        }

        [Fact]
        public void SortModulesToleratesExternalModulesWhichAreNotInTheContainer()
        {
            var externalModule = new ExternalScriptModule("http://test.com/test.js");
            var container = new ModuleContainer<ScriptModule>(Enumerable.Empty<ScriptModule>());
            var results = container.SortModules(new[] { externalModule });
            results.SequenceEqual(new[] { externalModule }).ShouldBeTrue();
        }

        [Fact]
        public void GivenModuleWithReferenceToAnotherModule_ModulesAreSortedInDependencyOrder()
        {
            var module1 = new Module("~/module1");
            var module2 = new Module("~/module2");
            module1.AddReferences(new[] {"~\\module2"});

            var container = new ModuleContainer<Module>(new[] {module1, module2});
            var sorted = container.SortModules(new[] { module1, module2});
            sorted.SequenceEqual(new[] { module2, module1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenModuleWithInvalid_ConstructorThrowsException()
        {
            var module1 = new Module("~/module1");
            module1.AddReferences(new[] { "~\\module2" });

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] {module1});
            });
            exception.Message.ShouldEqual("Reference error in module descriptor for \"~/module1\". Cannot find \"~/module2\".");
        }
    }
}