using System;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateModuleFactory_Tests
    {
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
