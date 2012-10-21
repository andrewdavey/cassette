using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Stylesheets;
using Cassette.TinyIoC;
using Should;
using Xunit;

namespace Cassette.Spriting
{
    public class SpritingIntegrationTest : IDisposable
    {
        readonly TinyIoCContainer container;
        readonly TempDirectory cache;
        readonly StylesheetBundle bundle;
        static FakeFileSystem sourceDirectory;

        public SpritingIntegrationTest()
        {
            container = CreateContainer();
            cache = new TempDirectory();
            InitDirectories();

            bundle = CreateStylesheetBundle();

            // SpriteImages expects image URLs to be expanded into absolute Cassette file URLs.
            ExpandUrls(bundle);
            SpriteImages(bundle);
        }

        [Fact]
        public void SingleSpriteImageCreatedInCacheSpritesDirectory()
        {
            using (var spriteImage = new Bitmap(Directory.GetFiles(Path.Combine(cache, "sprites")).Single()))
            {
                spriteImage.Height.ShouldEqual(20);
                spriteImage.Width.ShouldEqual(42); // Spritastic adds 1 pixel padding per image.
            }
        }

        [Fact]
        public void CssImageUrlsRewrittenToReferenceCachedSpriteImages()
        {
            using (var reader = new StreamReader(bundle.OpenStream()))
            {
                var css = reader.ReadToEnd();
                css.ShouldContain("url(/cassette.axd/cached/sprites/");
            }
        }

        void ExpandUrls(StylesheetBundle bundle)
        {
            container.Resolve<ExpandCssUrls>().Process(bundle);
        }

        void SpriteImages(StylesheetBundle bundle)
        {
            var processor = container.Resolve<SpriteImages>();
            processor.Process(bundle);
        }

        void InitDirectories()
        {
            var cassetteSettings = container.Resolve<CassetteSettings>();
            sourceDirectory.Add("~/image-b.png", BluePng());
            sourceDirectory.Add("~/image-a.png", RedPng());
            sourceDirectory.Add("~/asset.css", "");
            cassetteSettings.CacheDirectory = new FileSystemDirectory(cache);
        }

        static StylesheetBundle CreateStylesheetBundle()
        {
            var bundle = new StylesheetBundle("~");
            var asset = new StubAsset(
                "~/asset.css",
                ".a { background-image:url(image-a.png);background-repeat:no-repeat;width:20px;height:20px }\n" +
                ".b { background-image:url(image-b.png);background-repeat:no-repeat;width:20px;height:20px }"
                );
            bundle.Assets.Add(asset);
            return bundle;
        }

        static TinyIoCContainer CreateContainer()
        {
            var container = new TinyIoCContainer();
            new SpritingContainerConfiguration().Configure(container);
            sourceDirectory = new FakeFileSystem();
            var settings = new CassetteSettings
            {
                SourceDirectory = sourceDirectory
            };
            container.Register(settings);
            container.Register<IUrlGenerator>(new UrlGenerator(new VirtualDirectoryPrepender("/"), sourceDirectory, "cassette.axd/"));
            container.Register<IConfiguration<SpritingSettings>, DefaultSpritingSettingsConfiguration>();
            return container;
        }

        static byte[] BluePng()
        {
            using (var bitmap = new Bitmap(20, 20))
            using (var stream = new MemoryStream())
            {
                bitmap.SetPixel(0, 0, Color.Blue);
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        static byte[] RedPng()
        {
            using (var bitmap = new Bitmap(20, 20))
            using (var stream = new MemoryStream())
            {
                bitmap.SetPixel(0, 0, Color.Red);
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public void Dispose()
        {
            container.Dispose();
            cache.Dispose();
        }
    }
}