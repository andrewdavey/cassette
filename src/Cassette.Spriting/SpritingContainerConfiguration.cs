using System;
using Cassette.Spriting.Spritastic;
using Cassette.Spriting.Spritastic.Generator;
using Cassette.Spriting.Spritastic.Selector;
using Cassette.Spriting.Spritastic.Utilities;
using Cassette.TinyIoC;
using nQuant;

namespace Cassette.Spriting
{
    public class SpritingContainerConfiguration : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            // TODO: Apply IConfiguration<SpritingSettings>
            container.Register(new SpritingSettings { SpriteSizeLimit = 1000 * 1000, SpriteColorLimit = (int)Math.Pow(2,16) });

            container.Register<ICssImageExtractor, CssImageExtractor>();
            container.Register<ICssSelectorAnalyzer, CssSelectorAnalyzer>();
            container.Register<IPngOptimizer, PngOptimizer>();
            container.Register<IFileWrapper, FileWrapper>();
            container.Register<IWuQuantizer, WuQuantizer>();

            container.Register((c, n) => CreateSpriteGenerator(c));

            container.Register((c,n)=>
                new SpriteImages(
                    c.Resolve<CassetteSettings>(),
                    () => c.Resolve<ISpriteGenerator>()
                )
            );
        }

        static ISpriteGenerator CreateSpriteGenerator(TinyIoCContainer c)
        {
            var cssImageExtractor = c.Resolve<ICssImageExtractor>();
            var createSpriteManager = CreateSpriteManagerFactory(c);
            return new SpriteGenerator(cssImageExtractor, createSpriteManager);
        }

        static Func<string, ISpriteManager> CreateSpriteManagerFactory(TinyIoCContainer container)
        {
            var cassetteSettings = container.Resolve<CassetteSettings>();
            var settings = container.Resolve<SpritingSettings>();
            Func<byte[], string> generateSpriteUrl = container.Resolve<SpriteUrlGenerator>().CreateSpriteUrl;
            var pngOptimizer = container.Resolve<IPngOptimizer>();
            return path =>
            {
                var imageLoader = CreateImageLoader(cassetteSettings, container.Resolve<IUrlGenerator>());
                return new SpriteManager(settings, imageLoader, generateSpriteUrl, pngOptimizer);
            };
        }

        static ImageFileLoader CreateImageLoader(CassetteSettings cassetteSettings, IUrlGenerator urlGenerator)
        {
            return new ImageFileLoader(cassetteSettings.SourceDirectory, urlGenerator);
        }
    }
}