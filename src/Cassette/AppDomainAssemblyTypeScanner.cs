// This file is based on the AppDomainAssemblyTypeScanner from the most excellent Nancy project:
// https://github.com/NancyFx/Nancy/blob/master/src/Nancy/Bootstrapper/AppDomainAssemblyTypeScanner.cs
// Used under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Cassette
{
    /// <summary>
    /// Scans the app domain for assemblies and types.
    /// </summary>
    static class AppDomainAssemblyTypeScanner
    {
        static AppDomainAssemblyTypeScanner()
        {
            LoadNancyAssemblies();
        }

        /// <summary>
        /// Nancy core assembly
        /// </summary>
        static readonly Assembly CassetteAssembly = typeof(AppDomainAssemblyTypeScanner).Assembly;

        /// <summary>
        /// App domain type cache
        /// </summary>
        static IEnumerable<Type> _types;

        /// <summary>
        /// App domain assemblies cache
        /// </summary>
        static IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Indicates whether the cassette assemblies have already been loaded
        /// </summary>
        static bool _cassetteAssembliesLoaded;

        /// <summary>
        /// Gets app domain types.
        /// </summary>
        public static IEnumerable<Type> Types
        {
            get
            {
                return _types;
            }
        }

        /// <summary>
        /// Gets app domain types.
        /// </summary>
        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                return _assemblies;
            }
        }

        /// <summary>
        /// Load assemblies from a the app domain base directory matching a given wildcard.
        /// Assemblies will only be loaded if they aren't already in the appdomain.
        /// </summary>
        /// <param name="wildcardFilename">Wildcard to match the assemblies to load</param>
        public static void LoadAssemblies(string wildcardFilename)
        {
            LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory, wildcardFilename);
        }

        /// <summary>
        /// Load assemblies from a given directory matching a given wildcard.
        /// Assemblies will only be loaded if they aren't already in the appdomain.
        /// </summary>
        /// <param name="containingDirectory">Directory containing the assemblies</param>
        /// <param name="wildcardFilename">Wildcard to match the assemblies to load</param>
        public static void LoadAssemblies(string containingDirectory, string wildcardFilename)
        {
            UpdateAssemblies();

            var existingAssemblyPaths = _assemblies.Select(a => a.Location).ToArray();

            var unloadedAssemblies =
                Directory.GetFiles(containingDirectory, wildcardFilename).Where(
                    f => !existingAssemblyPaths.Contains(f, StringComparer.InvariantCultureIgnoreCase));

            foreach (var unloadedAssembly in unloadedAssemblies)
            {
                Assembly.Load(AssemblyName.GetAssemblyName(unloadedAssembly));
            }

            UpdateTypes();
        }

        /// <summary>
        /// Refreshes the type cache if additional assemblies have been loaded.
        /// Note: This is called automatically if assemblies are loaded using LoadAssemblies.
        /// </summary>
        public static void UpdateTypes()
        {
            UpdateAssemblies();

            _types = (from assembly in _assemblies
                     from type in assembly.GetExportedTypes()
                     where !type.IsAbstract
                     select type).ToArray();
        }

        /// <summary>
        /// Updates the assembly cache from the appdomain
        /// </summary>
        private static void UpdateAssemblies()
        {
            _assemblies = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
#if NET35
                           where !(assembly.ManifestModule is ModuleBuilder)
#else
                           where !assembly.IsDynamic
#endif
                           where !assembly.ReflectionOnly
                           select assembly).ToArray();
        }

        /// <summary>
        /// Loads any Nancy*.dll assemblies in the app domain base directory
        /// </summary>
        public static void LoadNancyAssemblies()
        {
            if (_cassetteAssembliesLoaded)
            {
                return;
            }

            LoadAssemblies(@"Cassette*.dll");

            _cassetteAssembliesLoaded = true;
        }

        /// <summary>
        /// Gets all types implementing a particular interface/base class
        /// </summary>
        /// <typeparam name="TType">Type to search for</typeparam>
        /// <param name="excludeInternalTypes">Whether to exclude types inside the core Nancy assembly</param>
        /// <returns>IEnumerable of types</returns>
        public static IEnumerable<Type> TypesOf<TType>(bool excludeInternalTypes = false)
        {
            var returnTypes = Types.Where(t => typeof(TType).IsAssignableFrom(t));

            if (excludeInternalTypes)
            {
                returnTypes = returnTypes.Where(t => t.Assembly != CassetteAssembly);
            }

            return returnTypes;
        }
    }
}