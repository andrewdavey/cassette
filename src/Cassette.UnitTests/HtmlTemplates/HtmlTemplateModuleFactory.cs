using System;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateModuleFactory_Tests
    {
        [Fact]
        public void CreateModule_ReturnsHtmlTemplateModuleWithPathSet()
        {
            var factory = new HtmlTemplateModuleFactory();
            var module = factory.CreateModule("~/test");
            module.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CreateExternalModuleThrowsException()
        {
            Assert.Throws<NotSupportedException>(delegate
            {
                new HtmlTemplateModuleFactory().CreateExternalModule("");
            });
        }
    }
}
