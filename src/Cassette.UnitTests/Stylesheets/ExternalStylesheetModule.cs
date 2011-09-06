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
        public void CanCreateNamedModule()
        {
            var module = new ExternalStylesheetModule("~/name", "http://url.com/");
            module.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateNamedModule_ThenPathIsAppRelative()
        {
            var module = new ExternalStylesheetModule("name", "http://url.com/");
            module.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateWithOnlyUrl_ThenPathIsUrl()
        {
            var module = new ExternalStylesheetModule("http://test.com/api.css");
            module.Path.ShouldEqual("http://test.com/api.css");
        }

        [Fact]
        public void CreateCacheManifestReturnsEmpty()
        {
            var module = new ExternalStylesheetModule("http://test.com/api.css");
            module.CreateCacheManifest().ShouldBeEmpty();
        }

        [Fact]
        public void RenderReturnsHtmlLinkElementWithUrlAsHref()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var html = module.Render();
            html.ToHtmlString().ShouldEqual("<link href=\"http://test.com/asset.css\" type=\"text/css\" rel=\"stylesheet\"/>");
        }

        [Fact]
        public void GivenMediaNotEmpty_RenderReturnsHtmlLinkElementWithMediaAttribute()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            module.Media = "print";
            var html = module.Render();
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
