using System;
using System.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class WrapHtmlTemplateInScriptBlock_Tests
    {
        HtmlTemplateModule module = new HtmlTemplateModule("test", Mock.Of<IFileSystem>());

        [Fact]
        public void ScriptBlockIdMatchesAssetFilenameWithoutExtension()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("test.htm");

            string output = Transform(asset, "<p>test</p>", "text/html");

            output.ShouldEqual(
                "<script id=\"test\" type=\"text/html\">" + Environment.NewLine +
                "<p>test</p>" + Environment.NewLine +
                "</script>"
            );
        }

        [Fact]
        public void ScriptBlockTypeCanBejQueryTmpl()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("test.htm");

            string output = Transform(asset, "<p>test</p>", "text/x-jquery-tmpl");

            output.ShouldEqual(
                "<script id=\"test\" type=\"text/x-jquery-tmpl\">" + Environment.NewLine +
                "<p>test</p>" + Environment.NewLine +
                "</script>"
            );
        }

        string Transform(Mock<IAsset> asset, string content, string contentType)
        {
            module.ContentType = contentType;
            var transformer = new WrapHtmlTemplateInScriptBlock(module);
            var getResult = transformer.Transform(() => content.AsStream(), asset.Object);
            using (var reader = new StreamReader(getResult()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
