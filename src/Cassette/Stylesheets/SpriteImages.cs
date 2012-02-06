using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.ReduceRequest.Reducer;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class SpriteImages : IBundleProcessor<StylesheetBundle>
    {
        readonly ICssImageTransformer cssImageTransformer;

        readonly IList<KeyValuePair<ImageMetadata, SpritedImage>> spriteList = new List<KeyValuePair<ImageMetadata, SpritedImage>>();

        SpriteContainer spriteContainer;

        public SpriteImages()
        {
            cssImageTransformer = new CssImageTransformer(new CssSelectorAnalyzer());
        }

        #region IBundleProcessor<StylesheetBundle> Members

        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            spriteContainer = new SpriteContainer();

            foreach (IAsset asset in bundle.Assets)
            {
                string css = ReadCss(asset.OpenStream());
                string currentDirectory = GetCurrentDirectory(asset);

                IEnumerable<BackgroundImageClass> extractImageUrls = cssImageTransformer.ExtractImageUrls(css);
                foreach (BackgroundImageClass image in extractImageUrls)
                {
                    string relativeFilename = GetImageFilename(image.ImageUrl, currentDirectory);

                    AddToSpriteList(image, settings.SourceDirectory, relativeFilename);

                    if (spriteContainer.Size >= settings.SpriteSizeLimit ||
                        spriteContainer.Colors >= settings.SpriteColorLimit)
                        Flush(settings);
                }

                if (extractImageUrls.Any())
                    asset.AddAssetTransformer(new SpriteReferenceTransformer(spriteList.Select(x => x.Value)));
            }

            Flush(settings);
            spriteList.ToList().ForEach(x => x.Value.Image.Dispose());
        }

        #endregion

        string GetCurrentDirectory(IAsset asset)
        {
            return asset.SourceFile.Directory.FullPath;
        }

        static string ReadCss(Stream openSourceStream)
        {
            using (var reader = new StreamReader(openSourceStream))
            {
                return reader.ReadToEnd();
            }
        }

        string GetImageFilename(string imageUrl, string currentDirectory)
        {
            string originalUrl = imageUrl.Trim('"', '\'');
            if (originalUrl.StartsWith("/"))
            {
                return PathUtilities.NormalizePath("~" + originalUrl);
            }
            return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(currentDirectory, originalUrl));
        }

        /// <summary>
        /// Add's images for spriting
        /// </summary>
        void AddToSpriteList(BackgroundImageClass image, IDirectory sourceDirectory, string relativeFilename)
        {
            var imageKey = new ImageMetadata(image);

            if (spriteList.Any(x => x.Key.Equals(imageKey)))
            {
                SpritedImage originalImage = spriteList.First(x => x.Key.Equals(imageKey)).Value;
                var clonedImage = new SpritedImage(originalImage.AverageColor, image, originalImage.Image)
                {
                    Position = originalImage.Position,
                    Url = originalImage.Url,
                    Metadata = imageKey
                };
                spriteContainer.AddImage(clonedImage);
                spriteList.Add(new KeyValuePair<ImageMetadata, SpritedImage>(imageKey, clonedImage));

                return;
            }

            SpritedImage spritedImage;
            KeyValuePair<ImageMetadata, SpritedImage> sprite = spriteList.FirstOrDefault(x => x.Value.CssClass.ImageUrl == image.ImageUrl);
            if (sprite.Value != null)
            {
                image.IsSprite = true;
                sprite.Value.CssClass.IsSprite = true;
            }

            try
            {
                spritedImage = spriteContainer.AddImage(image, () => sourceDirectory.GetFile(relativeFilename).ReadFully());
                spritedImage.Metadata = imageKey;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("There were errors reducing {0}", image.ImageUrl), ex);
            }

            spriteList.Add(new KeyValuePair<ImageMetadata, SpritedImage>(imageKey, spritedImage));
        }

        /// <summary>
        /// Flushes saved all images waiting to be sprited.
        /// </summary>
        void Flush(CassetteSettings cassetteSettings)
        {
            if (spriteContainer.Size > 0)
            {
                using (var spriteWriter = new SpriteWriter(spriteContainer.Width, spriteContainer.Height))
                {
                    int offset = 0;
                    foreach (SpritedImage image in spriteContainer)
                    {
                        spriteWriter.WriteImage(image.Image);
                        image.Position = offset;
                        offset += image.Image.Width + 1;
                    }
                    byte[] bytes = spriteWriter.GetBytes("image/png");

      
                    var optimizeImageTransformer = new OptimizeImageTransformer(cassetteSettings);

                    // Optipng
                    if (cassetteSettings.ImageQuantizationEnabled)
                        bytes = optimizeImageTransformer.QuantizeImage(bytes);

                    
                    bytes = optimizeImageTransformer.OptimizePng(bytes);

                    string filename = GetSpriteUrl(bytes);

                    IFile file = cassetteSettings.SpriteDirectory.GetFile(filename);
                    using (Stream stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }

                    foreach (SpritedImage image in spriteContainer)
                    {
                        //TODO: Hack need to get a relative filename from the css to the sprites folder
                        image.Url = "/sprites/" + filename;
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

            spriteContainer = new SpriteContainer();
        }

        string GetSpriteUrl(byte[] bytes)
        {
            string hash = SHA1.Create().ComputeHash(bytes).ToHexString();

            return hash + ".png";
        }
    }
}