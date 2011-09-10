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
        public void ProcessCallsProcessor()
        {
            var module = new ExternalScriptModule("http://test.com/asset.js");
            var processor = new Mock<IModuleProcessor<ScriptModule>>();
            var application = Mock.Of<ICassetteApplication>();

            module.Processor = processor.Object;
            module.Process(application);

            processor.Verify(p => p.Process(module, application));
        }

        [Fact]
        public void CanActAsAModuleSourceOfItself()
        {
            var module = new ExternalScriptModule("http://test.com/asset.js");
            var result = (module as IModuleSource<ScriptModule>).GetModules(Mock.Of<IModuleFactory<ScriptModule>>(), Mock.Of<ICassetteApplication>());

            result.SequenceEqual(new[] { module }).ShouldBeTrue();
        }

        [Fact]
        public void GivenModuleHasName_ContainsPathOfThatNameReturnsTrue()
        {
            var module = new ExternalScriptModule("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            module.ContainsPath("~/GoogleMapsApi").ShouldBeTrue();
        }

        [Fact]
        public void GivenModuleHasName_PathIsApplicationRelativeFormOfTheName()
        {
            var module = new ExternalScriptModule("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            module.Path.ShouldEqual("~/GoogleMapsApi");
        }

        [Fact]
        public void GivenModuleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var module = new ExternalScriptModule("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            module.ContainsPath("https://maps-api-ssl.google.com/maps/api/js?sensor=false").ShouldBeTrue();
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
            module.Path.ShouldEqual("~/api");
        }
    }
}