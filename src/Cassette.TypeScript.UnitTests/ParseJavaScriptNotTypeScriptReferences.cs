using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class ParseJavaScriptNotTypeScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToJavaScriptAssetInBundleAndIgnoresTypeScriptReferences()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.js"); // Remember TS files compile down to JS files

            var typeScriptSource = @"
/// <reference path=""../../../../typings/jquery/jquery.d.ts"" />
/// <reference path=""~/Scripts/jquery.js"" />
// @reference ""~/bundles/bundle1""
// @reference ~/bundles/bundle2

$(document).ready(function () {
});";
            asset.Setup(a => a.OpenStream())
                 .Returns(typeScriptSource.AsStream());
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseJavaScriptNotTypeScriptReferences();
            processor.Process(bundle);

            asset.Verify(a => a.AddReference("../../../../typings/jquery/jquery.d.ts", 2), Times.Never());
            asset.Verify(a => a.AddReference("~/Scripts/jquery.js", 3));
            asset.Verify(a => a.AddReference("~/bundles/bundle1", 4));
            asset.Verify(a => a.AddReference("~/bundles/bundle2", 5));
        }
    }
}

