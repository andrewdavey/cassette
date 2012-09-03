using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cassette.Spriting.Spritastic.ImageLoad;
using Cassette.Spriting.Spritastic.Parser;

namespace Cassette.Spriting.Spritastic.Generator
{
    class SpriteContainer : ISpriteContainer
    {
        protected readonly IList<SpritedImage> Images = new List<SpritedImage>();
        private readonly ISpritingSettings settings;
        private readonly HashSet<int> uniqueColors = new HashSet<int>();
        internal readonly Dictionary<string, byte[]> DownloadedImages = new Dictionary<string, byte[]>();

        public SpriteContainer(IImageLoader imageLoader, ISpritingSettings settings)
        {
            ImageLoader = imageLoader;
            this.settings = settings;
        }

        public void AddImage (SpritedImage image)
        {
            Images.Add(image);
        }

        public SpritedImage AddImage (BackgroundImageClass image)
        {
            byte[] imageBytes;
            if(DownloadedImages.ContainsKey(image.ImageUrl) && image.IsSprite)
                imageBytes = DownloadedImages[image.ImageUrl];
            else
            {
                imageBytes = ImageLoader.GetImageBytes(image.ImageUrl);
                if(image.IsSprite)
                    DownloadedImages.Add(image.ImageUrl, imageBytes);
            }
            Bitmap bitmap;

            using (var originalBitmap = new Bitmap(new MemoryStream(imageBytes)))
            {
                var width = image.Width ?? originalBitmap.Width; //cliped width of original image
                var height = image.Height ?? originalBitmap.Height;
                using (var writer = new SpriteWriter(width, height))
                {
                    var x = image.XOffset.Offset < 0 ? Math.Abs(image.XOffset.Offset) : 0; //offset on original
                    var y = image.YOffset.Offset < 0 ? Math.Abs(image.YOffset.Offset) : 0;
                    var imageWidth = width; //canvas width
                    if (width + x > originalBitmap.Width)   
                        width = originalBitmap.Width - x;
                    var imageHeight = height;
                    if (height + y > originalBitmap.Height)
                        height = originalBitmap.Height - y;
                    var offsetX = 0;
                    var offsetY = 0;
                    if (image.XOffset.PositionMode == PositionMode.Direction)
                    {
                        switch (image.XOffset.Direction)
                        {
                            case Direction.Left:
                                image.XOffset = new Position { PositionMode = PositionMode.Percent, Offset = 0};
                                break;
                            case Direction.Center:
                                image.XOffset = new Position { PositionMode = PositionMode.Percent, Offset = 50};
                                break;
                            case Direction.Right:
                                image.XOffset = new Position { PositionMode = PositionMode.Percent, Offset = 100 };
                                break;
                        }
                    }
                    if (image.YOffset.PositionMode == PositionMode.Direction)
                        switch (image.YOffset.Direction)
                        {
                            case Direction.Top:
                                image.YOffset = new Position { PositionMode = PositionMode.Percent, Offset = 0 };
                                break;
                            case Direction.Center:
                                image.YOffset = new Position { PositionMode = PositionMode.Percent, Offset = 50 };
                                break;
                            case Direction.Bottom:
                                image.YOffset = new Position { PositionMode = PositionMode.Percent, Offset = 100 };
                                break;
                        }
                    if (image.XOffset.PositionMode == PositionMode.Percent)
                    {
                        if (originalBitmap.Width > imageWidth)
                            x = (int)Math.Round((originalBitmap.Width - imageWidth) * (image.XOffset.Offset / 100f), 0);
                        else
                            offsetX = (int)Math.Round((imageWidth - originalBitmap.Width) * (image.XOffset.Offset / 100f), 0);
                    }
                    else if (image.XOffset.PositionMode == PositionMode.Unit && image.XOffset.Offset > 0)
                    {
                        offsetX = image.XOffset.Offset;
                        if (originalBitmap.Width + offsetX > imageWidth)
                            width = imageWidth - offsetX;
                    }

                    if (image.YOffset.PositionMode == PositionMode.Percent)
                    {
                        if (originalBitmap.Height > imageHeight)
                            y = (int)Math.Round((originalBitmap.Height - height) * (image.YOffset.Offset / 100f), 0);
                        else
                            offsetY = (int)Math.Round((imageHeight - originalBitmap.Height) * (image.YOffset.Offset / 100f), 0);
                    }

                    try
                    {
                        using (var bm = originalBitmap.Clone(new Rectangle(x, y, width, height), originalBitmap.PixelFormat))
                        {
                            writer.WriteImage(bm, offsetX, offsetY);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw new InvalidOperationException(string.Format("Unable to Add {0} to Sprite.", image.ImageUrl));
                    }
                    bitmap = writer.SpriteImage;
                    if ((originalBitmap.Width * originalBitmap.Height) > (bitmap.Width * bitmap.Height))
                        Size += writer.GetBytes("image/png").Length;
                    else
                        Size += imageBytes.Length;
                }
            }
            var avgColor = settings.IsFullTrust ? GetColors(bitmap) : 0;
            var spritedImage = new SpritedImage(avgColor, image, bitmap);
            Images.Add(spritedImage);
            Width += bitmap.Width + 1;
            if (Height < bitmap.Height) Height = bitmap.Height;
            return spritedImage;
        }


        private int GetColors(Bitmap bitmap)
        {
            long totalArgb = 0;
            var total = 0;
            var data = bitmap.LockBits(Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height),
                                            ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                var byteLength = data.Stride < 0 ? -data.Stride : data.Stride;
                var buffer = new Byte[byteLength * bitmap.Height];
                var offset = 0;
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var argb = BitConverter.ToInt32(buffer, offset);
                        uniqueColors.Add(argb);
                        totalArgb += argb;
                        ++total;
                        offset += 4;
                    }
                }

                return (int)(totalArgb / total);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        public int Size { get; private set; }
        public int Colors
        {
            get { return uniqueColors.Count; }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public IImageLoader ImageLoader { get; set; }

        public IEnumerator<SpritedImage> GetEnumerator()
        {
            return Images.OrderBy(x => x.AverageColor).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
