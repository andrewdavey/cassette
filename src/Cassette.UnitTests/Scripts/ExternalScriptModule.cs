using System;
using System.Linq;
using Cassette.ModuleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptModule_Tests
    {
        [Fact]
        public void GivenExternalScriptModuleWithFallback_ThenRenderReturnsFallbackScript()
        {
            var module = new ExternalScriptModule("api", "http://test.com/api.js", "!window.api", "/api.js");
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ToHtmlString().ShouldEqual(
                @"<script src=""http://test.com/api.js"" type=""text/javascript""></script>" + 
                Environment.NewLine + 
                @"<script type=""text/javascript"">!window.api && document.write(unescape('%3Cscript src=""/api.js""%3E%3C/script%3E'))</script>"
            );
        }

        [Fact]
        public void IsPersistentIsFalse()
        {
            new ExternalScriptModule("http://test.com/api.js").IsPersistent.ShouldBeFalse();
        }

        [Fact]
        public void ProcessDoesNothing()
        {
            var module = new ExternalScriptModule("http://test.com/asset.js");
            var processor = new Mock<IModuleProcessor<ScriptModule>>();
            module.Processor = processor.Object;
            module.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(It.IsAny<ScriptModule>(), It.IsAny<ICassetteApplication>()), Times.Never());
        }

        [Fact]
        public void CanActAsAModuleSourceOfItself()
        {
            var module = new ExternalScriptModule("http://test.com/asset.js");
            var result = (module as IModuleSource<ScriptModule>).GetModules(Mock.Of<IModuleFactory<ScriptModule>>(), Mock.Of<ICassetteApplication>());

            result.LastWriteTimeMax.ShouldEqual(DateTime.MinValue);
            result.Modules.SequenceEqual(new[] { module }).ShouldBeTrue();
        }

        [Fact]
        public void GivenNoFallbackUrl_ThenRenderReturnsTheScriptElement()
        {
            var module = new ExternalScriptModule("http://test.com/asset.js");
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ToHtmlString().ShouldEqual("<script src=\"http://test.com/asset.js\" type=\"text/javascript\"></script>");
        }
    }

    public class ExternalScriptModule_ConstructorConstraints
    {
        [Fact]
        public void UrlRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptModule("api", null, "!window.api", "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptModule("api", "", "!window.api", "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptModule("api", " ", "!window.api", "/api.js");
            });
        }

        [Fact]
        public void JavaScriptConditionRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptModule("api", "http://test.com/api.js", null, "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptModule("api", "http://test.com/api.js", "", "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptModule("api", "http://test.com/api.js", " ", "/api.js");
            });
        }

        [Fact]
        public void FallbackUrlRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptModule("api", "http://test.com/api.js", "!window.api", null);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptModule("api", "http://test.com/api.js", "!window.api", "");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptModule("api", "http://test.com/api.js", "!window.api", " ");
            });
        }

        [Fact]
        public void CanCreateAdHocExternalScriptModuleWithOnlyAUrl()
        {
            var module = new ExternalScriptModule("http://test.com/api.js");
            module.Path.ShouldEqual("http://test.com/api.js");
        }

        [Fact]
        public void CanCreateNamedExternalScriptModule()
        {
            var module = new ExternalScriptModule("api", "http://test.com/api.js");
            module.Path.ShouldEqual("api");
        }
    }
}
