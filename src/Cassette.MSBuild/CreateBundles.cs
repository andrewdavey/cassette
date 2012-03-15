using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cassette.IO;
using Cassette.Manifests;
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
            using (var outputStream = OpenOutputFile())
            {
                var task = CreateTaskImplementation(outputStream);
                task.Execute();
            }
            return true;
        }

        FileStream OpenOutputFile()
        {
            return File.Open(Output, FileMode.Create, FileAccess.Write, FileShare.None);
        }

        CreateBundlesImplementation CreateTaskImplementation(Stream outputStream)
        {
            var configurationFactory = CreateConfigurationFactory();
            var writer = new CassetteManifestWriter(outputStream);
            return new CreateBundlesImplementation(
                configurationFactory,
                writer,
                new FileSystemDirectory(Environment.CurrentDirectory)
            );
        }

        AssemblyScanningCassetteConfigurationFactory CreateConfigurationFactory()
        {
            var assemblies = LoadAssemblies();
            return new AssemblyScanningCassetteConfigurationFactory(assemblies);
        }

        IEnumerable<Assembly> LoadAssemblies()
        {
            return (
                from assembly in Assemblies
                select Assembly.LoadFrom(assembly)
            ).ToArray();
        }
    }
}