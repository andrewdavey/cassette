using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Hosting;
using Cassette.Configuration;
using Cassette.Utilities;

namespace Cassette.ReduceRequest.Reducer
{
    public class SpriteManager : ISpriteManager
    {
        ISpriteContainer spriteContainer;
        readonly IList<KeyValuePair<ImageMetadata, SpritedImage>> spriteList = new List<KeyValuePair<ImageMetadata, SpritedImage>>();

        // TODO: Moved into cassette settings
        const int SpriteSizeLimit = 50000;
        const int SpriteColorLimit = 5000;

        readonly CassetteSettings settings;

        public SpriteManager(CassetteSettings settings)
        {
            this.settings = settings;
            spriteContainer = new SpriteContainer(settings);
        }

        public virtual void Add(BackgroundImageClass image)
        {
            var imageKey = new ImageMetadata(image);
 
            if (spriteList.Any(x => x.Key.Equals(imageKey)))
            {
                var originalImage = spriteList.First(x => x.Key.Equals(imageKey)).Value;
                var clonedImage = new SpritedImage(originalImage.AverageColor, image, originalImage.Image) { Position = originalImage.Position, Url = originalImage.Url, Metadata = imageKey };
                spriteContainer.AddImage(clonedImage);
                spriteList.Add(new KeyValuePair<ImageMetadata, SpritedImage>(imageKey, clonedImage));

                return;
            }

            SpritedImage spritedImage;
            var sprite = spriteList.FirstOrDefault(x => x.Value.CssClass.ImageUrl == image.ImageUrl);
            if (sprite.Value != null)
            {
                image.IsSprite = true;
                sprite.Value.CssClass.IsSprite = true;
            }

            try
            {
                spritedImage = spriteContainer.AddImage(image);
                spritedImage.Metadata = imageKey;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("There were errors reducing {0}", image.ImageUrl), ex);
            }

            spriteList.Add(new KeyValuePair<ImageMetadata, SpritedImage>(imageKey, spritedImage));

            if (spriteContainer.Size >= SpriteSizeLimit || spriteContainer.Colors >= SpriteColorLimit)
                Flush();
        }

        public void Flush()
        {
            if (spriteContainer.Size > 0)
            {
                using (var spriteWriter = new SpriteWriter(spriteContainer.Width, spriteContainer.Height))
                {
                    var offset = 0;
                    foreach (var image in spriteContainer)
                    {
                        spriteWriter.WriteImage(image.Image);
                        image.Position = offset;
                        offset += image.Image.Width + 1;
                    }
                    var bytes = spriteWriter.GetBytes("image/png");

                    var url = GetSpriteUrl(bytes);

                    var fullPath = HostingEnvironment.MapPath("~" + url);

                    var fileName = Path.GetFileName(fullPath);
                    var directory = fullPath.Replace(fileName, "");

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(fullPath, bytes);
                    foreach (var image in spriteContainer)
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
                }
            }

            spriteContainer = new SpriteContainer(settings);
        }

        public Guid SpritedCssKey { get; set; }

        private string GetSpriteUrl(byte[] bytes)
        {
            var hash = SHA1.Create().ComputeHash(bytes).ToHexString();

            return "/app_data/" + hash + ".png";
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
            Flush();
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