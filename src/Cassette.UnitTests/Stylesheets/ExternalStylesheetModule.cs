using System;
using System.Linq;
using Cassette.ModuleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule_Tests
    {
        [Fact]
        public void CreateCacheManifestReturnsExternalModuleElement()
        {
            var element = new ExternalStylesheetModule("http://test.com/api.css").CreateCacheManifest().Single();
            element.Name.LocalName.ShouldEqual("ExternalModule");
        }

        [Fact]
        public void CreateCacheManifestReturnsExternalModuleElementWithUrlAttribute()
        {
            var element = new ExternalStylesheetModule("http://test.com/api.css").CreateCacheManifest().Single();
            element.Attribute("Url").Value.ShouldEqual("http://test.com/api.css");
        }

        [Fact]
        public void CreateCacheManifestReturnsExternalModuleElementWithContentTypeAttribute()
        {
            var element = new ExternalStylesheetModule("http://test.com/api.js").CreateCacheManifest().Single();
            element.Attribute("ContentType").Value.ShouldEqual("text/css");
        }

        [Fact]
        public void CreateCacheManifestReturnsExternalModuleElementWithMediaAttribute()
        {
            var module = new ExternalStylesheetModule("http://test.com/api.js") {Media = "print"};
            var element = module.CreateCacheManifest().Single();
            element.Attribute("Media").Value.ShouldEqual("print");
        }

        [Fact]
        public void RenderReturnsHtmlLinkElementWithUrlAsHref()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ToHtmlString().ShouldEqual("<link href=\"http://test.com/asset.css\" type=\"text/css\" rel=\"stylesheet\"/>");
        }

        [Fact]
        public void GivenMediaNotEmpty_RenderReturnsHtmlLinkElementWithMediaAttribute()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            module.Media = "print";
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ToHtmlString().ShouldEqual("<link href=\"http://test.com/asset.css\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>");
        }

        [Fact]
        public void ProcessDoesNothing()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var processor = new Mock<IModuleProcessor<StylesheetModule>>();
            module.Processor = processor.Object;
            module.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(It.IsAny<StylesheetModule>(), It.IsAny<ICassetteApplication>()), Times.Never());
        }

        [Fact]
        public void CanActAsAModuleSourceOfItself()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var result = (module as IModuleSource<StylesheetModule>).GetModules(Mock.Of<IModuleFactory<StylesheetModule>>(), Mock.Of<ICassetteApplication>());
            
            result.SequenceEqual(new[] { module }).ShouldBeTrue();
        }
    }
}
