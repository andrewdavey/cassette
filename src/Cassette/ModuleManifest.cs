using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    public class ModuleManifest
    {
        readonly IEnumerable<Module> modules;
        readonly StringComparer pathComparer = StringComparer.OrdinalIgnoreCase;

        public ModuleManifest(IEnumerable<Module> modules)
        {
            this.modules = modules;
        }

        public IEnumerable<Module> Modules
        {
            get { return modules; }
        }

        public ModuleDifference[] CompareTo(ModuleManifest other)
        {
            var modulePathComparer = new ModulePathComparer(pathComparer);
            var currentModules = new HashSet<Module>(modules, modulePathComparer);
            var oldModules = new HashSet<Module>(other.Modules, modulePathComparer);

            var addedModules = currentModules.Except(oldModules, modulePathComparer);
            var added = addedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Added));

            var deletedModules = oldModules.Except(currentModules, modulePathComparer);            
            var deleted = deletedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Deleted));

            // Changed module means old and current have the same path, but different hash.
            // Use Join to pair up the modules by path.
            // Then use Where to filter out those with same hash.
            var changedModules = oldModules.Join(currentModules, m => m.Path, m => m.Path, (old, current) => new { old, current })
                .Where(c => c.current.Equals(c.old) == false);
            // So deleted the old modules and add the current modules.
            var changes = changedModules.SelectMany(c => new[] 
                { 
                    new ModuleDifference(c.old, ModuleDifferenceType.Deleted), 
                    new ModuleDifference(c.current, ModuleDifferenceType.Added) 
                }
            );

            return added.Concat(deleted).Concat(changes).ToArray();
        }

        class ModulePathComparer : IEqualityComparer<Module>
        {
            public ModulePathComparer(StringComparer pathComparer)
            {
                this.pathComparer = pathComparer;
            }

            readonly StringComparer pathComparer;

            public bool Equals(Module x, Module y)
            {
                return pathComparer.Equals(x.Path, y.Path);
            }

            public int GetHashCode(Module module)
            {
                return module.Path.GetHashCode();
            }
        }
    }
}