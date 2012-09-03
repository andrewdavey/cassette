using System;
using System.Linq;
using Cassette.Spriting.Spritastic.Generator;
using Cassette.Spriting.Spritastic.Utilities;

namespace Cassette.Spriting.Spritastic
{
    class SpriteGenerator : ISpriteGenerator
    {
        private readonly ICssImageExtractor cssImageExtractor;
        private readonly Func<string, ISpriteManager> createSpriteManager;

        /// <param name="createSpriteManager">Function that creates an <see cref="ISpriteManager"/> for the given CSS URL or absolute filename.</param>
        public SpriteGenerator(ICssImageExtractor cssImageExtractor, Func<string, ISpriteManager> createSpriteManager)
        {
            this.cssImageExtractor = cssImageExtractor;
            this.createSpriteManager = createSpriteManager;
        }

        /// <param name="cssContent">The CSS content to sprite.</param>
        /// <param name="cssPath">The URL or absolute file name of the CSS being sprited. Used to resolve image URLs that are relative to the CSS file.</param>
        public SpritePackage GenerateFromCss(string cssContent, string cssPath)
        {
            var newImages = cssImageExtractor.ExtractImageUrls(cssContent);
            using (var spriteManager = createSpriteManager(cssPath))
            {
                foreach (var imageUrl in newImages)
                {
                    Tracer.Trace("Adding {0}", imageUrl.ImageUrl);
                    spriteManager.Add(imageUrl);
                    Tracer.Trace("Finished adding {0}", imageUrl.ImageUrl);
                }
                var sprites = spriteManager.Flush();
                var newCss = spriteManager.Aggregate(
                    cssContent,
                    (current, spritedImage) => spritedImage.InjectIntoCss(current)
                );
                return new SpritePackage(newCss, sprites, spriteManager.Errors);
            }
        }
    }
}
