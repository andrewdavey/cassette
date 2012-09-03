using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassette.Spriting.Spritastic.ImageLoad;
using Cassette.Spriting.Spritastic.Parser;
using Cassette.Spriting.Spritastic.Utilities;

namespace Cassette.Spriting.Spritastic.Generator
{
    class SpriteManager : ISpriteManager
    {
        protected ISpriteContainer SpriteContainer;
        readonly ISpritingSettings config;
        private readonly Func<byte[], string> generateSpriteUrl;
        readonly IPngOptimizer pngOptimizer;
        readonly IList<KeyValuePair<ImageMetadata, SpritedImage>> spriteList = new List<KeyValuePair<ImageMetadata, SpritedImage>>();
        readonly IList<Sprite> finalSprites = new List<Sprite>(); 

        public SpriteManager(ISpritingSettings config, IImageLoader imageLoader, Func<byte[], string> generateSpriteUrl, IPngOptimizer pngOptimizer)
        {
            this.pngOptimizer = pngOptimizer;
            this.config = config;
            this.generateSpriteUrl = generateSpriteUrl;
            SpriteContainer = new SpriteContainer(imageLoader, config);
            Errors = new List<SpriteException>();
        }

        // For testing access
        internal IList<KeyValuePair<ImageMetadata, SpritedImage>> SpriteList
        {
            get { return spriteList; }
        }

        public IImageLoader ImageLoader
        {
            get { return SpriteContainer.ImageLoader; }
            set { SpriteContainer.ImageLoader = value; }
        }

        public Predicate<BackgroundImageClass> ImageExclusionFilter { get; set; }

        public IList<SpriteException> Errors { get; internal set; }

        public virtual void Add(BackgroundImageClass image)
        {
            if (ImageExclusionFilter != null && ImageExclusionFilter(image)) return;

            var imageKey = new ImageMetadata(image);
            
            if (spriteList.Any(x => x.Key.Equals(imageKey)))
            {
                var originalImage = spriteList.First(x => x.Key.Equals(imageKey)).Value;
                var clonedImage = new SpritedImage(originalImage.AverageColor, image, originalImage.Image) { Position = originalImage.Position, Url = originalImage.Url, Metadata = imageKey };
                spriteList.Add(new KeyValuePair<ImageMetadata, SpritedImage>(imageKey, clonedImage));
                return;
            }
            SpritedImage spritedImage;
            var sprite = spriteList.FirstOrDefault(x => x.Value.CssClass.ImageUrl == image.ImageUrl);
            if(sprite.Value != null)
            {
                image.IsSprite = true;
                sprite.Value.CssClass.IsSprite = true;
            }
            try
            {
                spritedImage = SpriteContainer.AddImage(image);
                spritedImage.Metadata = imageKey;
            }
            catch (Exception ex)
            {
                var message = string.Format("There were errors reducing {0}", image.ImageUrl);
                Tracer.Trace(message);
                Tracer.Trace(ex.ToString());
                var wrappedException = new SpriteException(image.OriginalClassString, message, ex);
                Errors.Add(wrappedException);
                return;
            }
            spriteList.Add(new KeyValuePair<ImageMetadata, SpritedImage>(imageKey, spritedImage));
            if (SpriteContainer.Size >= config.SpriteSizeLimit || (SpriteContainer.Colors >= config.SpriteColorLimit && !config.ImageQuantizationDisabled && !config.ImageOptimizationDisabled))
                Flush();
        }

        public virtual IList<Sprite> Flush()
        {
            if(SpriteContainer.Size > 0)
            {
                Tracer.Trace("Beginning to Flush sprite");
                using (var spriteWriter = new SpriteWriter(SpriteContainer.Width, SpriteContainer.Height))
                {
                    var offset = 0;
                    foreach (var image in SpriteContainer)
                    {
                        spriteWriter.WriteImage(image.Image);
                        image.Position = offset;
                        offset += image.Image.Width + 1;
                    }
                    var bytes = spriteWriter.GetBytes("image/png");
                    byte[] optBytes;
                    try
                    {
                        optBytes = (config.ImageOptimizationDisabled || !config.IsFullTrust) ? bytes : pngOptimizer.OptimizePng(bytes, config.ImageOptimizationCompressionLevel, config.ImageQuantizationDisabled);
                    }
                    catch (OptimizationException optEx)
                    {
                        optBytes = bytes;
                        var message = string.Format("Errors optimizing. Received Error: {0}", optEx.Message);
                        Tracer.Trace(message);
                        Errors.Add(new SpriteException(message, optEx));
                    }
                    var url = generateSpriteUrl(optBytes);
                    foreach (var image in SpriteContainer)
                    {
                        image.Url = url;
                        foreach (var dupImage in spriteList)
                        {
                            if (dupImage.Key.Equals(image.Metadata) && dupImage.Value.Position == -1)
                            {
                                dupImage.Value.Position = image.Position;
                                dupImage.Value.Url = image.Url;
                            }
                        }
                    }
                    finalSprites.Add(new Sprite(url, optBytes));
                }
                Tracer.Trace("Finished Flushing sprite");
            }
            SpriteContainer = new SpriteContainer(ImageLoader, config);
            return finalSprites;
        }

        public IEnumerator<SpritedImage> GetEnumerator()
        {
            return spriteList.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            spriteList.ToList().ForEach(x => x.Value.Image.Dispose());
        }

        public struct ImageMetadata
        {
            public ImageMetadata(BackgroundImageClass image) : this()
            {
                Url = image.ImageUrl;
                Width = image.Width ?? 0;
                Height = image.Height ?? 0;
                XOffset = image.XOffset.Offset;
                YOffset = image.YOffset.Offset;
            }

            public int Width { get; set; }
            public int Height { get; set; }
            public int XOffset { get; set; }
            public int YOffset { get; set; }
            public string Url { get; set; }
        }
    }
}