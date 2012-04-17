using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
#if NET35
using System.Reflection.Emit;
#endif

namespace Cassette
{
    class AssemblyScanner
    {
        readonly IEnumerable<Assembly> assemblies;

        public AssemblyScanner(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = assemblies
                .Where(assembly => AssemblyIsNotIgnored(assembly) && IsNotDynamic(assembly))
                .ToArray();
        }

        public Type[] GetAllTypes()
        {
            return (
                from assembly in assemblies
                where AssemblyIsNotIgnored(assembly) && IsNotDynamic(assembly)
                from type in assembly.GetExportedTypes()
                where !type.IsAbstract
                select type
            ).ToArray();
        }

        public string HashAssemblies()
        {
            using (var allHashes = new MemoryStream())
            using (var sha1 = SHA1.Create())
            {
                var filenames = assemblies
                    .Where(assembly => AssemblyIsNotIgnored(assembly) && IsNotDynamic(assembly))
                    .Select(assembly => assembly.Location)
                    .OrderBy(filename => filename);

                foreach (var filename in filenames)
                {
                    using (var file = File.OpenRead(filename))
                    {
                        var hash = sha1.ComputeHash(file);
                        allHashes.Write(hash, 0, hash.Length);
                    }
                }
                allHashes.Position = 0;
                return Convert.ToBase64String(sha1.ComputeHash(allHashes));
            }
        }

        static bool AssemblyIsNotIgnored(Assembly assembly)
        {
            return !IgnoredAssemblies.Any(ignore => ignore(assembly));
        }

        static readonly List<Func<Assembly, bool>> IgnoredAssemblies = new List<Func<Assembly, bool>>
        {
            assembly => assembly.FullName.StartsWith("Microsoft.", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("mscorlib,", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("System.", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("System,", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("IronPython", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("IronRuby", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("CR_ExtUnitTest", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("CR_VSTest", StringComparison.InvariantCulture),
            assembly => assembly.FullName.StartsWith("DevExpress.CodeRush", StringComparison.InvariantCulture)
        };

        static bool IsNotDynamic(Assembly assembly)
        {
#if NET35
            return !(assembly.ManifestModule is ModuleBuilder);
#else
            return !assembly.IsDynamic;
#endif
        }
    }
}