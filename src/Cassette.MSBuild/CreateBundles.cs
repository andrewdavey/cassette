using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    [LoadInSeparateAppDomain]
    [Serializable]
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

        /// <summary>
        /// The virtual path of the directory that contains the web application.
        /// Defaults to "/".
        /// </summary>
        public string AppVirtualPath { get; set; }

        /// <summary>
        /// When true, non-bundled files such as images referenced by stylesheets are also copied to the output directory. Default is false.
        /// </summary>
        public bool IncludeOtherFiles { get; set; }

     
        public override bool Execute()
        {
            AssignPropertyDefaultsIfMissing();
            MakePathsAbsolute();

            Log.LogMessage("Source directory = {0}", Source);
            Log.LogMessage("Bin directory = {0}", Bin);
            Log.LogMessage("Output directory = {0}", Output);
            Log.LogMessage("App virtual path = {0}", AppVirtualPath);
            Log.LogMessage("Include other files = {0}", IncludeOtherFiles);

            if (!AppVirtualPath.EndsWith("/"))
            {
                Log.LogError("AppVirtualPath must end with \"\".");
                return false;
            }

            // Execution will load assemblies. When running this task from a Visual Studio build, the DLLs would then be locked.
            // So we must run the command in a separate AppDomain.
            // This means the assemblies can be unlocked by unloading the new AppDomain when finished.
            CreateBundlesCommand.ExecuteInSeparateAppDomain(new CreateBundlesCommand(Source, Bin, Output, AppVirtualPath, IncludeOtherFiles, Log));
            
            return true;
        }


        private void AssignPropertyDefaultsIfMissing()
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

            if (string.IsNullOrEmpty(AppVirtualPath))
            {
                AppVirtualPath = "/";
            }
        }

        private void MakePathsAbsolute()
        {
            Source = Path.Combine(Environment.CurrentDirectory, Source);
            Bin = Path.Combine(Source, Bin);
            Output = Path.Combine(Source, Output);
        }
    }
}