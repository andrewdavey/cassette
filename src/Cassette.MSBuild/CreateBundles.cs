using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    [Serializable]
    [LoadInSeparateAppDomain]
    public class CreateBundles : AppDomainIsolatedTask
    {
        /// <summary>
        /// File names of assemblies containing Cassette configuration classes.
        /// </summary>
        [Required]
        public string[] Assemblies { get; set; }

        /// <summary>
        /// File name to save the Cassette manifest as.
        /// </summary>
        [Required]
        public string Output { get; set; }

        public override bool Execute()
        {
            var host = new MSBuildHost();
            host.Initialize();
            host.Execute();
            host.Dispose();
            return true;
        }
    }
}