using Cassette.Stylesheets;
using Xunit;

namespace Cassette.Spriting
{
    public class SpriteImagesTests
    {
        [Fact]
        public void DontSpriteWhenDebugging()
        {
            var bundle = new StylesheetBundle("~");
            var asset = new StubAsset(
                "~/asset.css",
                ".a { background-image:url(image-a.png);width:20px;height:20px }\n" +
                ".b { background-image:url(image-b.png);width:20px;height:20px }"
            );
            bundle.Assets.Add(asset);

            var settings = new CassetteSettings
            {
                IsDebuggingEnabled = true,
                CacheDirectory = new FakeFileSystem()
            };

            var processor = new SpriteImages(settings, () => null);
            processor.Process(bundle);
            // Would throw exception if it tried to sprite the CSS.
        }
    }
}