using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplateWithJQueryTmpl_Tests
    {
        [Fact]
        public void TransformReturnsJavaScriptThatAddsNamedTemplate()
        {
            var bundle = new HtmlTemplateBundle("~");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Path).Returns("~/asset.htm");
            var transformer = new RegisterTemplateWithJQueryTmpl(bundle, new HtmlTemplateIdBuilder());

            var result = transformer.Transform("TEMPLATE", asset.Object);

            result.ShouldEqual("jQuery.template('asset', TEMPLATE);" + Environment.NewLine);
        }
    }
}
