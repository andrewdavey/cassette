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
        public string Source { get; set; }

        [Required]
        public string Assemblies { get; set; }

        [Required]
        public string Output { get; set; }

        public override bool Execute()
        {
            using (var host = new MSBuildHost(Source, Assemblies, Output))
            {
                host.Initialize();
                host.Execute();
            }
            return true;
        }
    }
}