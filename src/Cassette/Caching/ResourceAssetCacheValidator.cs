using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cassette.Caching
{
    public class ResourceAssetCacheValidator : IAssetCacheValidator
    {
        /// <summary>
        /// Mapping of resource file name to assembly
        /// </summary>
        static readonly IDictionary<string, Assembly> resourceAssemblyMap;

        static ResourceAssetCacheValidator()
        {
            resourceAssemblyMap = FindAllResources();
        }

        /// <summary>
        /// Find all the resources in all assemblies in the current AppDomain
        /// </summary>
        /// <returns>Resources and their associated assembly</returns>
        private static IDictionary<string, Assembly> FindAllResources()
        {
            var resourcesMap = new Dictionary<string, Assembly>();
            
            var resources =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                where IsValidResourceAssembly(assembly)
                from resource in assembly.GetManifestResourceNames()
                select new { Assembly = assembly, ResourceName = resource };

            // Duplicate keys shouldn't occur, but ensure they don't cause any errors!
            foreach (var resource in resources)
            {
                resourcesMap[resource.ResourceName] = resource.Assembly;
            }

            return resourcesMap;
        }

        /// <summary>
        /// Determines whether the specified assembly should be treated as a valid assembly for 
        /// embedded resources in Cassette
        /// </summary>
        /// <param name="assembly">Assembly to check</param>
        /// <returns><c>true</c> if it should be counted</returns>
        private static bool IsValidResourceAssembly(Assembly assembly)
        {
            return
                // Exclude dynamic assemblies
                !assembly.IsDynamic
                // Exclude system assemblies
                && assembly.FullName != "System"
                && assembly.FullName != "mscorlib"
                && !assembly.FullName.StartsWith("System.")
                && !assembly.FullName.StartsWith("Microsoft.");
        }

        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            Assembly assembly;
            resourceAssemblyMap.TryGetValue(assetPath.Replace("~/", string.Empty), out assembly);

            // If we're not sure what assembly this file belongs to, just assume the cache is fine
            if (assembly == null)
                return true;

            // Check the last modified date of the assembly
            var lastModified = File.GetLastWriteTimeUtc(assembly.Location);
            return lastModified <= asOfDateTime;
        }
    }
}