//using System;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using RequestReduce.Configuration;
//using nQuant;

//namespace RequestReduce.Utilities
//{
//    public interface IPngOptimizer
//    {
//        byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled);
//    }

//    public class PngOptimizer : IPngOptimizer
//    {
//        private readonly IFileWrapper fileWrapper;
//        private readonly IRRConfiguration configuration;
//        private readonly IWuQuantizer wuQuantizer;
//        private readonly string optiPngLocation;

//        public PngOptimizer(IFileWrapper fileWrapper, IRRConfiguration configuration, IWuQuantizer wuQuantizer)
//        {
//            this.fileWrapper = fileWrapper;
//            this.configuration = configuration;
//            this.wuQuantizer = wuQuantizer;
//            var dllDir = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
//            optiPngLocation = string.Format("{0}\\OptiPng.exe", dllDir);
//        }

//        public byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled)
//        {
//                var optimizedBytes = bytes;
//                if (!imageQuantizationDisabled)
//                {
//                    using(var unQuantized = new Bitmap(new MemoryStream(bytes)))
//                    {
//                        using(var quantized = wuQuantizer.QuantizeImage(unQuantized, 10, 5))
//                        {
//                            var memStream = new MemoryStream(); 
//                            quantized.Save(memStream, ImageFormat.Png);
//                            optimizedBytes = memStream.GetBuffer();
//                        }
//                    }
//                }

//                 if (fileWrapper.FileExists(optiPngLocation))
//                 {
//                    var scratchFile = string.Format("{0}\\scratch-{1}.png", configuration.SpritePhysicalPath, Hasher.Hash(bytes));
//                    try
//                    {
//                        fileWrapper.Save(optimizedBytes, scratchFile);
//                        var arg = String.Format(@"-fix -o{1} ""{0}""", scratchFile, compressionLevel);
//                        InvokeExecutable(arg, optiPngLocation);
//                        optimizedBytes = fileWrapper.GetFileBytes(scratchFile);
//                    }
//                    finally
//                    {
//                        fileWrapper.DeleteFile(scratchFile);
//                    }
//                 }

//                 return optimizedBytes;
//        }

//        private void InvokeExecutable(string arguments, string executable)
//        {
//            using(var process = new Process())
//            {
//                process.StartInfo = new ProcessStartInfo
//                                        {
//                    UseShellExecute = false,
//                    RedirectStandardOutput = true,
//                    CreateNoWindow = true,
//                    FileName = executable,
//                    Arguments = arguments
//                };
//                process.Start();
//                process.StandardOutput.ReadToEnd();
//                process.WaitForExit(10000);
//                if(!process.HasExited)
//                {
//                    process.Kill();
//                    throw new OptimizationException
//                        (string.Format("Unable to optimize image using executable {0} with arguments {1}", 
//                        executable, arguments));
//                }
//            }
//        }
//    }

//    public class OptimizationException : Exception
//    {
//        public OptimizationException(string message) : base(message)
//        {
//        }
//    }
//}
