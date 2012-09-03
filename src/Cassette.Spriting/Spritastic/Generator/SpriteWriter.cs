using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Cassette.Spriting.Spritastic.Generator
{
    class SpriteWriter : IDisposable
    {
        public Bitmap SpriteImage { get; private set; }
        private readonly Graphics drawingSurface = null;

        public SpriteWriter(int surfaceWidth, int surfaceHeight)
        {
            try
            {
                SpriteImage = new Bitmap(surfaceWidth, surfaceHeight);
            }
            catch (Exception e)
            {
                throw e;
            }
            drawingSurface = Graphics.FromImage(SpriteImage);
            drawingSurface.Clear(Color.Transparent);
        }


        public void WriteImage(Bitmap image)
        {
            WriteImage(image, 0, 0);
        }

        public void WriteImage(Bitmap image, int offsetX, int offsetY)
        {
            try
            {
                drawingSurface.DrawImage(image, new Rectangle(OffsetWidth + offsetX, offsetY, image.Width, image.Height));
            }
            catch (Exception e)
            {
                
                throw e;
            }
            OffsetWidth += image.Width + 1;
        }

        public byte[] GetBytes(string mimeType)
        {
            using (var spriteEncoderParameters = new EncoderParameters(1))
            {
                spriteEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90);
                using (var stream = new MemoryStream())
                {
                    SpriteImage.Save(stream, ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == mimeType), spriteEncoderParameters);
                    return stream.GetBuffer();
                }
            }
        }

        public int OffsetWidth { get; private set; }

        public void Dispose()
        {
            if (drawingSurface != null)
                drawingSurface.Dispose();
        }
    }
}
