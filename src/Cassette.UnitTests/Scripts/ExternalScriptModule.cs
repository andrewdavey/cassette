using System;
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
        public void CanCreateAdHocExternalScriptModule()
        {
            var module = new ExternalScriptModule("http://test.com/api.js");
            module.Directory.ShouldEqual("");
        }

        [Fact]
        public void CanCreateExternalScriptModuleWithOnlyAUrl()
        {
            var module = new ExternalScriptModule("api", "http://test.com/api.js");
            module.Directory.ShouldEqual("api");
        }
    }
}
