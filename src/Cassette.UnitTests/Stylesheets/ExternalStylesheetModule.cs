using System;
using System.Linq;
using Moq;
using Should;
using Xunit;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule_Tests
    {
        [Fact]
        public void IsPersistentReturnsFalse()
        {
            new ExternalStylesheetModule("http://test.com/asset.css").IsPersistent.ShouldBeFalse();
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
            
            result.LastWriteTimeMax.ShouldEqual(DateTime.MinValue);
            result.Modules.SequenceEqual(new[] { module }).ShouldBeTrue();
        }
    }
}
