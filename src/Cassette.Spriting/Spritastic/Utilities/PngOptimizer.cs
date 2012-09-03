using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using nQuant;

namespace Cassette.Spriting.Spritastic.Utilities
{
    class PngOptimizer : IPngOptimizer
    {
        private readonly IFileWrapper fileWrapper;
        private readonly IWuQuantizer wuQuantizer;
        private readonly string optiPngLocation;

        public PngOptimizer(IFileWrapper fileWrapper, IWuQuantizer wuQuantizer)
        {
            this.fileWrapper = fileWrapper;
            this.wuQuantizer = wuQuantizer;
            var dllDir = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            optiPngLocation = string.Format("{0}\\OptiPng.exe", dllDir);
        }

        public byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled)
        {
            var optimizedBytes = bytes;
            if (!imageQuantizationDisabled)
            {
                using (var unQuantized = new Bitmap(new MemoryStream(bytes)))
                {
                    using (var quantized = wuQuantizer.QuantizeImage(unQuantized, 10, 5))
                    {
                        var memStream = new MemoryStream();
                        quantized.Save(memStream, ImageFormat.Png);
                        optimizedBytes = memStream.GetBuffer();
                    }
                }
            }

            if (fileWrapper.FileExists(optiPngLocation))
            {
                var scratchFile = Path.GetTempFileName();
                fileWrapper.Save(optimizedBytes, scratchFile);
                var arg = String.Format(@"-fix -o{1} ""{0}""", scratchFile, compressionLevel);
                InvokeExecutable(arg, optiPngLocation);
                optimizedBytes = fileWrapper.GetFileBytes(scratchFile);
            }

            return optimizedBytes;
        }

        private void InvokeExecutable(string arguments, string executable)
        {
            using(var process = new Process())
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
                if(!process.HasExited)
                {
                    process.Kill();
                    throw new OptimizationException
                        (string.Format("Unable to optimize image using executable {0} with arguments {1}", 
                        executable, arguments));
                }
            }
        }
    }
}
