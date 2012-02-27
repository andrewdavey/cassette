using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Utilities;
using nQuant;

namespace Cassette.Stylesheets
{
    public class OptimizeImageTransformer : IAssetTransformer
    {
        readonly IWuQuantizer wuQuantizer;
        readonly CassetteSettings cassetteSettings;

        public OptimizeImageTransformer( CassetteSettings cassetteSettings)
        {
            this.cassetteSettings = cassetteSettings;

            if (cassetteSettings.ImageQuantizationEnabled)
                wuQuantizer = new WuQuantizer();
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            //TODO: Implement transforms to potentially handle images to process which are not in sprites
            throw new NotImplementedException();
        }

        public byte[] QuantizeImage(byte[] image)
        {
            using (var unQuantized = new Bitmap(new MemoryStream(image)))
            {
                using (var quantized = wuQuantizer.QuantizeImage(unQuantized, 10, 5))
                {
                    var memStream = new MemoryStream();
                    quantized.Save(memStream, ImageFormat.Png);
                    image = memStream.GetBuffer();
                }
            }

            return image;
        }

        public byte[] OptimizePng(byte[] image)
        {
            if (!File.Exists(cassetteSettings.OptiPngPath))
                return image;

            var tempFileName = GetSpriteUrl(image);
            var tempFile = cassetteSettings.SpriteDirectory.GetFile(tempFileName);
            try
            {
                using (var stream = tempFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    // Save the temp image which optipng will use work on.
                    stream.Write(image, 0, image.Length);
                    stream.Flush();
                    stream.Close();
                }

                var arg = String.Format(@"-fix -o{1} ""{0}""", tempFile.FullSystemPath, cassetteSettings.ImageOptimizationCompressionLevel);
                InvokeExecutable(arg, cassetteSettings.OptiPngPath);

                image = tempFile.ReadFully();
            }
            finally
            {
                tempFile.Delete();
            }

            return image;
        }

        private void InvokeExecutable(string arguments, string executable)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    FileName = executable,
                    Arguments = arguments
                };

                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);

                if (!process.HasExited)
                {
                    process.Kill();
                    throw new Exception(string.Format("Failed optimizing images. Arguments {0}", arguments));
                }
            }
        }

        private static string GetSpriteUrl(byte[] bytes)
        {
            string hash = SHA1.Create().ComputeHash(bytes).ToHexString();

            return hash + ".png";
        }
    }
}