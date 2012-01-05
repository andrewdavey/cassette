using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cassette.Configuration;
using Cassette.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette
{
    public class CreateBundles : AppDomainIsolatedTask
    {
        [Required]
        public string AssemblyName { get; set; }

        public override bool Execute()
        {
            var root = Environment.CurrentDirectory;
            var assembly = Assembly.LoadFrom(Path.Combine(root, "bin", AssemblyName + ".dll"));
            
            var configurations = 
                from type in assembly.GetExportedTypes()
                where type.IsClass && !type.IsAbstract && typeof(ICassetteConfiguration).IsAssignableFrom(type)
                select (ICassetteConfiguration)Activator.CreateInstance(type);

            var settings = new CassetteSettings("")
            {
                SourceDirectory = new FileSystemDirectory(root)
            };

            var bundles = new BundleCollection(settings);
            foreach (var configuration in configurations)
            {
                configuration.Configure(bundles, settings);
            }

            foreach (var bundle in bundles)
            {
                Log.LogMessage("{0}: {1}", bundle.GetType().Name, bundle.Path);
                foreach (var asset in bundle.Assets)
                {
                    Log.LogMessage("  " + asset.SourceFile.FullPath);
                }

                bundle.Process(settings);
                using (var s = bundle.OpenStream())
                {
                    Log.LogMessage("Size: {0} bytes", s.Length);
                }
            }

            return true;
        }
    }
}
