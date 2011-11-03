﻿#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class ReferenceBuilder_Reference_Tests
    {
        public ReferenceBuilder_Reference_Tests()
        {
            moduleContainer = new Mock<IModuleContainer<ScriptModule>>();
            moduleFactory = new Mock<IModuleFactory<ScriptModule>>();
            application = new Mock<ICassetteApplication>();
            builder = new ReferenceBuilder<ScriptModule>(moduleContainer.Object, moduleFactory.Object, Mock.Of<IPlaceholderTracker>(), application.Object);

            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns<IEnumerable<Module>>(ms => ms);
        }

        readonly ReferenceBuilder<ScriptModule> builder;
        readonly Mock<IModuleContainer<ScriptModule>> moduleContainer;
        readonly Mock<IModuleFactory<ScriptModule>> moduleFactory;
        readonly Mock<ICassetteApplication> application;

        [Fact]
        public void WhenAddReferenceToModuleDirectory_ThenGetModulesReturnTheModule()
        {
            var module = new ScriptModule("~/test");
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns(new[] { module })
                           .Verifiable();
            builder.Reference("test", null);

            var modules = builder.GetModules(null).ToArray();

            modules[0].ShouldBeSameAs(module);
            moduleContainer.Verify();
        }

        [Fact]
        public void WhenAddReferenceToModuleDirectoryWithLocation_ThenGetModulesThatLocationReturnTheModule()
        {
            var module = new ScriptModule("~/test");
            module.Location = "body";
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns(new[] { module })
                           .Verifiable();
            builder.Reference("test", null);

            var modules = builder.GetModules("body").ToArray();

            modules[0].ShouldBeSameAs(module);
            moduleContainer.Verify();
        }

        [Fact]
        public void OnlyModulesMatchingLocationAreReturnedByGetModules()
        {
            var module1 = new ScriptModule("~/test1");
            var module2 = new ScriptModule("~/test2");
            module1.Location = "body";
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test1"))
                           .Returns(module1);
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test2"))
                           .Returns(module2);
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns(new[] { module1 });
            builder.Reference("test1", null);
            builder.Reference("test2", null);

            var modules = builder.GetModules("body").ToArray();
            modules.Length.ShouldEqual(1);
            modules[0].ShouldBeSameAs(module1);
        }

        [Fact]
        public void WhenAddReferenceToNonExistentModule_ThenThrowException()
        {
            moduleContainer.Setup(c => c.FindModuleContainingPath("~\\test")).Returns((ScriptModule)null);

            Assert.Throws<ArgumentException>(delegate
            {
                builder.Reference("test", null);
            });
        }

        [Fact]
        public void GivenModuleAReferencesModuleB_WhenAddReferenceToModuleA_ThenGetModulesReturnsBoth()
        {
            var moduleA = new ScriptModule("~/a");
            var moduleB = new ScriptModule("~/b");

            moduleContainer.Setup(c => c.FindModuleContainingPath("~/a"))
                           .Returns(moduleA);
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns(new[] { moduleB, moduleA });

            builder.Reference("a", null);

            builder.GetModules(null).SequenceEqual(new[] { moduleB, moduleA }).ShouldBeTrue();
        }

        [Fact]
        public void WhenAddReferenceToUrl_ThenGetModulesReturnsAnExternalModule()
        {
            moduleFactory.Setup(f => f.CreateExternalModule("http://test.com/test.js"))
                         .Returns(new ExternalScriptModule("http://test.com/test.js"));
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns<IEnumerable<Module>>(all => all);

            builder.Reference("http://test.com/test.js", null);

            var module = builder.GetModules(null).First();
            module.ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void WhenAddReferenceToHttpsUrl_ThenGetModulesReturnsAnExternalModule()
        {
            moduleFactory.Setup(f => f.CreateExternalModule("https://test.com/test.js"))
                         .Returns(new ExternalScriptModule("https://test.com/test.js"));
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns<IEnumerable<Module>>(all => all);

            builder.Reference("https://test.com/test.js", null);

            var module = builder.GetModules(null).First();
            module.ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void WhenAddReferenceToProtocolRelativeUrl_ThenGetModulesReturnsAnExternalModule()
        {
            moduleFactory.Setup(f => f.CreateExternalModule("//test.com/test.js"))
                         .Returns(new ExternalScriptModule("//test.com/test.js"));
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns<IEnumerable<Module>>(all => all);

            builder.Reference("//test.com/test.js", null);

            var module = builder.GetModules(null).First();
            module.ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void WhenAddReferenceWithLocation_ThenGetModulesForThatLocationReturnsTheModule()
        {
            var module = new ScriptModule("~/test");
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test"))
                           .Returns(module);
            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns(new[] { module });
            builder.Reference("test", "body");

            builder.GetModules("body").SequenceEqual(new[] { module}).ShouldBeTrue();
        }

        [Fact]
        public void GivenLocationAlreadyRendered_WhenAddReferenceToThatLocation_ThenExceptionThrown()
        {
            var module = new ScriptModule("~/test");
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test"))
                           .Returns(module);

            builder.Render("test");

            Assert.Throws<InvalidOperationException>(delegate
            {
                builder.Reference("~/test", "test");
            });
        }

        [Fact]
        public void GivenLocationAlreadyRenderedButHtmlRewrittingEnabled_WhenAddReferenceToThatLocation_ThenModuleStillAdded()
        {
            application.SetupGet(a => a.HtmlRewritingEnabled)
                       .Returns(true);
            var module = new ScriptModule("~/test");
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/test"))
                           .Returns(module);
            builder.Render("test");

            builder.Reference("~/test", "test");

            builder.GetModules("test").First().ShouldBeSameAs(module);
        }
    }

    public class ReferenceBuilder_Render_Tests
    {
        public ReferenceBuilder_Render_Tests()
        {
            moduleContainer = new Mock<IModuleContainer<Module>>();
            moduleFactory = new Mock<IModuleFactory<Module>>();
            placeholderTracker = new Mock<IPlaceholderTracker>();
            application = Mock.Of<ICassetteApplication>();
            referenceBuilder = new ReferenceBuilder<Module>(moduleContainer.Object, moduleFactory.Object, placeholderTracker.Object, application);

            moduleContainer.Setup(c => c.IncludeReferencesAndSortModules(It.IsAny<IEnumerable<Module>>()))
                           .Returns<IEnumerable<Module>>(ms => ms);

            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<string>>()))
                              .Returns("output");
        }

        readonly ReferenceBuilder<Module> referenceBuilder;
        readonly Mock<IPlaceholderTracker> placeholderTracker;
        readonly ICassetteApplication application;
        readonly Mock<IModuleContainer<Module>> moduleContainer;
        readonly Mock<IModuleFactory<Module>> moduleFactory;

        [Fact]
        public void GivenAddReferenceToPath_WhenRender_ThenModuleRenderOutputReturned()
        {
            var module = new Module("~/stub");
            moduleContainer.Setup(c => c.FindModuleContainingPath(It.IsAny<string>()))
                           .Returns(module);

            referenceBuilder.Reference("test");

            var html = referenceBuilder.Render();

            html.ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRenderWithLocation_ThenModuleRenderOutputReturned()
        {
            var module = new Mock<Module>("~/stub");
            module.Setup(m => m.Render(application))
                  .Returns("output");
            moduleContainer.Setup(c => c.FindModuleContainingPath(It.IsAny<string>()))
                           .Returns(module.Object);
            referenceBuilder.Reference("test");

            var html = referenceBuilder.Render("body");

            html.ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToTwoPaths_WhenRender_ThenModuleRenderOutputsSeparatedByNewLinesReturned()
        {
            var module1 = new Mock<Module>("~/stub1");
            module1.Setup(m => m.Render(application))
                   .Returns("output1");
            var module2 = new Mock<Module>("~/stub2");
            module2.Setup(m => m.Render(application))
                   .Returns("output2");
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/stub1"))
                           .Returns(module1.Object);
            moduleContainer.Setup(c => c.FindModuleContainingPath("~/stub2"))
                           .Returns(module2.Object);

            referenceBuilder.Reference("~/stub1");
            referenceBuilder.Reference("~/stub2");

            Func<string> createHtml = null;
            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<string>>()))
                .Returns("output")
                .Callback<Func<string>>(f => createHtml = f);

            referenceBuilder.Render();

            createHtml().ShouldEqual("output1" + Environment.NewLine + "output2");
        }
    }
}

