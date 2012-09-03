using Cassette.Stylesheets;
using Xunit;

namespace Cassette.Spriting
{
    public class SpriteImagesTests
    {
        [Fact]
        public void _()
        {
            var bundle = new StylesheetBundle("~");
            var asset = new StubAsset("~/asset.css", 
                ".a { background-image:url(image-a.png);width:20px;height:20px }\n" +
                ".b { background-image:url(image-b.png);width:20px;height:20px }");
            bundle.Assets.Add(asset);

            var cacheDirectory = new FakeFileSystem();

            var processor = new SpriteImages(cacheDirectory);
            processor.Process(bundle);

            //CacheDirectoryContains "~/sprites/hash.png"
        }
    }
}