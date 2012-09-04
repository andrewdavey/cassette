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
            var lazySettings = new Lazy<SpritingSettings>(() => CreateSpritingSettings(container));
            container.Register((c, n) => lazySettings.Value);

            container.Register<ICssImageExtractor, CssImageExtractor>();
            container.Register<ICssSelectorAnalyzer, CssSelectorAnalyzer>();
            container.Register<IPngOptimizer, PngOptimizer>();
            container.Register<IFileWrapper, FileWrapper>();
            container.Register<IWuQuantizer, WuQuantizer>();

            container.Register((c, n) => CreateSpriteGenerator(c));

            RegisterSpriteImagesBundleProcessor(container);
        }

        static void RegisterSpriteImagesBundleProcessor(TinyIoCContainer container)
        {
            // The SpriteImages constructor is internal (to avoid having to expose all of Spritastic).
            // TinyIoC won't have access to the internal constructor.
            // Therefore we have to a delegate to express SpriteImages creation.
            container.Register((c, n) =>
                new SpriteImages(
                    c.Resolve<CassetteSettings>(),
                    c.Resolve<ISpriteGenerator>
                )
            );
        }

        static SpritingSettings CreateSpritingSettings(TinyIoCContainer container)
        {
            var settings = new SpritingSettings();
            container
                .ResolveAll<IConfiguration<SpritingSettings>>()
                .OrderByConfigurationOrderAttribute()
                .Configure(settings);
            return settings;
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