using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    [Serializable]
    [LoadInSeparateAppDomain]
    public class CreateBundles : AppDomainIsolatedTask
    {
        [Required]
        public string Input { get; set; }

        /// <summary>
        /// File name to save the Cassette manifest as.
        /// </summary>
        [Required]
        public string Output { get; set; }

        public override bool Execute()
        {
            using (var host = new MSBuildHost(Input, Output))
            {
                host.Initialize();
                host.Execute();
            }
            return true;
        }
    }
}