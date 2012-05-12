using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    [Serializable]
    [LoadInSeparateAppDomain]
    public class CreateBundles : AppDomainIsolatedTask
    {
        /// <summary>
        /// The root directory of the web application.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The directory containing the web application assemblies. Default is "bin".
        /// </summary>
        public string Bin { get; set; }

        /// <summary>
        /// The directory to save the created bundles to. Default is "cassette-cache".
        /// </summary>
        public string Output { get; set; }

        public override bool Execute()
        {
            AssignPropertyDefaultsIfMissing();
            MakePathsAbsolute();

            using (var host = new MSBuildHost(Source, Bin, Output))
            {
                host.Initialize();
                host.Execute();
            }
            return true;
        }

        void AssignPropertyDefaultsIfMissing()
        {
            if (string.IsNullOrEmpty(Source))
            {
                Source = Environment.CurrentDirectory;
            }

            if (string.IsNullOrEmpty(Bin))
            {
                Bin = "bin";
            }

            if (string.IsNullOrEmpty(Output))
            {
                Output = "cassette-cache";
            }
        }

        void MakePathsAbsolute()
        {
            Bin = Path.Combine(Source, Bin);
            Output = Path.Combine(Source, Output);
        }
    }
}