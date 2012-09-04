using System;
using Cassette.Spriting.Spritastic;
using Cassette.Spriting.Spritastic.Generator;
using Cassette.Spriting.Spritastic.ImageLoad;
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
            RegisterSpritingSettings(container);
            RegisterSpritasticServices(container);
            RegisterSpriteGenerator(container);
            RegisterSpriteImagesBundleProcessor(container);
        }

        static void RegisterSpritingSettings(TinyIoCContainer container)
        {
            // SpritingSettings needs to be a singleton,
            // but the created instance needs to be configured with 
            // any implementations of IConfiguration<SpritingSettings>.
            // So use a Lazy object to ensure one instance is created and configured.
            var lazySettings = new Lazy<SpritingSettings>(
                () => CreateAndConfigureSpritingSettings(container)
            );
            container.Register((c, n) => lazySettings.Value);
        }

        static void RegisterSpritasticServices(TinyIoCContainer container)
        {
            container.Register<ICssImageExtractor, CssImageExtractor>();
            container.Register<ICssSelectorAnalyzer, CssSelectorAnalyzer>();
            container.Register<IPngOptimizer, PngOptimizer>();
            container.Register<IFileWrapper, FileWrapper>();
            container.Register<IWuQuantizer, WuQuantizer>();
            container.Register<IImageLoader>(
                (c, n) => new ImageFileLoader(
                    c.Resolve<CassetteSettings>().SourceDirectory,
                    c.Resolve<IUrlGenerator>()
                )
            );
        }

        static void RegisterSpriteGenerator(TinyIoCContainer container)
        {
            container.Register((c, n) => CreateSpriteGenerator(c));
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

        static SpritingSettings CreateAndConfigureSpritingSettings(TinyIoCContainer container)
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
            return path =>
            {
                var settings = container.Resolve<SpritingSettings>();
                var imageLoader = container.Resolve<IImageLoader>();
                Func<byte[], string> generateSpriteUrl = container.Resolve<SpriteUrlGenerator>().CreateSpriteUrl;
                var pngOptimizer = container.Resolve<IPngOptimizer>();
                return new SpriteManager(settings, imageLoader, generateSpriteUrl, pngOptimizer)
                {
                    ImageExclusionFilter = image => !settings.ShouldSpriteImage(image.ImageUrl)
                };
            };
        }
    }
}