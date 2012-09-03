using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.Spriting.Spritastic;
using Cassette.Spriting.Spritastic.Generator;
using Cassette.Spriting.Spritastic.ImageLoad;
using Cassette.Spriting.Spritastic.Selector;
using Cassette.Spriting.Spritastic.Utilities;
using Should;
using Xunit;

namespace Cassette.Spriting
{
    public class Test
    {
        [Fact]
        public void TestSpriting()
        {
            var loader = new TestLoader();
            var spritingSettings = new SpritingSettings
            {
                SpriteSizeLimit = 100 * 100,
                SpriteColorLimit = 4096,
                ImageOptimizationDisabled = true,
                ImageQuantizationDisabled = true
            };
            var generator = new SpriteGenerator(
                new CssImageExtractor(new CssSelectorAnalyzer()),
                path => new SpriteManager(
                    spritingSettings,
                    loader,
                    bytes => "/url",
                    new NullOptimizer()
                )
            );
            var package = generator.GenerateFromCss(
                ".a { background: url(a.png) no-repeat; width: 10px; height: 10px; }\n" +
                ".b { background: url(b.png) no-repeat; width: 10px; height: 10px; }", 
                "/test.css");
            package.Exceptions.ShouldBeEmpty();
            package.GeneratedCss.ShouldEqual(
                ".a { background: url(/url) no-repeat; width: 10px; height: 10px; ;background-position: -0px 0;}\n" +
                ".b { background: url(/url) no-repeat; width: 10px; height: 10px; ;background-position: -11px 0;}"
            );

            var s = new MemoryStream(package.Sprites[0].Image);
            using(var bmp = new Bitmap(s))
            {
                bmp.Width.ShouldEqual(22);
                bmp.Height.ShouldEqual(10);
            }
        }
    }

    public class TestLoader : IImageLoader
    {
        public string BasePath { get; set; }

        public byte[] GetImageBytes(string url)
        {
            using (var b = new Bitmap(10, 10))
            using (var stream = new MemoryStream())
            {
                b.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }

    public class NullOptimizer : IPngOptimizer
    {
        public byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled)
        {
            return bytes;
        }
    }
}
