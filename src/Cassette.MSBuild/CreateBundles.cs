using System.IO;
using Cassette.Manifests;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    public class CreateBundles : AppDomainIsolatedTask
    {
        /// <summary>
        /// The web application assembly filename.
        /// </summary>
        [Required]
        public string Assembly { get; set; }

        [Required]
        public string Output { get; set; }

        public override bool Execute()
        {
            using (var outputStream = File.OpenWrite(Output))
            {
                var task = CreateTaskImplementation(outputStream);
                task.Execute();
            }
            return true;
        }

        CreateBundlesImplementation CreateTaskImplementation(FileStream outputStream)
        {
            var assembly = System.Reflection.Assembly.LoadFrom(Assembly);
            var configurationFactory = new AssemblyScanningCassetteConfigurationFactory(new[] { assembly });
            var writer = new CassetteManifestWriter(outputStream);
            return new CreateBundlesImplementation(configurationFactory, writer);
        }
    }
}