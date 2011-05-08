using System;
using System.Collections.Generic;
using System.Linq;

namespace Knapsack
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
            var deletedModules = oldModules.Except(currentModules, modulePathComparer);
            var changedModules = oldModules.Zip(currentModules, (old, current) => new { old, current })
                .Where(x => modulePathComparer.Equals(x.old, x.current))
                .Where(x => x.old.Equals(x.current) == false)
                .Select(c => c.current);

            var added = addedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Added));
            var deleted = deletedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Deleted));
            var changed = changedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Changed));
            
            return added.Concat(deleted).Concat(changed).ToArray();
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