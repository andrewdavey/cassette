using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cassette.Scripts;

namespace Perf
{
    class Program
    {
        static void Main(string[] args)
        {
            var assetContent = string.Join("\r\n", Enumerable.Range(0, 1000).Select(i => "var x" + i + "={};"));

            var temp = Path.GetFullPath(".");
            for (int i = 0; i < 200; i++)
            {
                var bundlePath = Path.Combine(temp, "bundle" + i);
                if (Directory.Exists(bundlePath)) continue;
                Directory.CreateDirectory(bundlePath);
                for (int j = 0; j < 30; j++)
                {
                    var assetFilename = Path.Combine(bundlePath, "asset" + j + ".js");
                    File.WriteAllText(assetFilename, assetContent);
                }
            }

            using (var host = new TestableWebHost(temp, () => null, true))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.AddPerSubDirectory<ScriptBundle>("")
                ));

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                host.Initialize();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
                stopWatch.Restart();

                var wait = new ManualResetEvent(false);

                host.Bundles.Changed += (sender, eventArgs) =>
                {
                    Console.WriteLine("Changed after " + stopWatch.ElapsedMilliseconds + "ms");
                    wait.Set();
                };
                Console.WriteLine("Changing");
                File.WriteAllText(Path.Combine(temp, "bundle1", "asset1.js"), "var x = 1;");

                wait.WaitOne();
            }
        }
    }
}
