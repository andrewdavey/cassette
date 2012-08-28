using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cassette.Scripts;

namespace Perf
{
    class Program
    {
        static void Main(string[] args)
        {
            var assetContent = string.Join("\r\n", Enumerable.Range(0, 1000).Select(i => "var x" + i + "={};"));

            using (var temp = new TempDirectory())
            {
                for (int i = 0; i < 200; i++)
                {
                    var bundlePath = Path.Combine(temp, "bundle" + i);
                    Directory.CreateDirectory(bundlePath);
                    for (int j = 0; j < 30; j++)
                    {
                        var assetFilename = Path.Combine(bundlePath, "asset" + j + ".js");
                        File.WriteAllText(assetFilename, assetContent);
                    }
                }

                Console.WriteLine("Assets written to disk. Start profiler, then press Enter.");
                Console.ReadLine();

                using (var host = new TestableWebHost(temp, () => null, true))
                {
                    host.AddBundleConfiguration(new BundleConfiguration(bundles =>
                        bundles.AddPerSubDirectory<ScriptBundle>("")
                    ));

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    host.Initialize();

                    File.WriteAllText(Path.Combine(temp, "bundle100", "asset15.js"), "var x = 1;");

                    Console.WriteLine(stopWatch.ElapsedMilliseconds);
                    stopWatch.Stop();
                }
            }
        }
    }
}
