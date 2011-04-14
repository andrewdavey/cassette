using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
    public class ModuleContainer
    {
        readonly Module[] modules;
        readonly Dictionary<string, Module> modulesByScriptPath;

        public ModuleContainer(IEnumerable<Module> modules)
        {
            this.modules = modules.ToArray();

            modulesByScriptPath = (
                from module in modules
                from script in module.Scripts
                select new { script.Path, module }
            ).ToDictionary(x => x.Path, x => x.module);
        }

        public Module FindModuleContainingScript(string scriptPath)
        {
            Module module;
            if (modulesByScriptPath.TryGetValue(scriptPath, out module))
            {
                return module;
            }

            return null;
        }

        public bool Contains(string modulePath)
        {
            return modules.Any(m => m.Path == modulePath);
        }

        public Module GetModule(string modulePath)
        {
            return modules.First(m => m.Path == modulePath);
        }

        public ModuleDifference[] CompareTo(ModuleContainer oldModuleContainer)
        {
            var pathComparer = new ModulePathComparer();
            var currentModules = new HashSet<Module>(modules, new ModulePathComparer());
            var oldModules = new HashSet<Module>(oldModuleContainer.modules, new ModulePathComparer());

            var addedModules = currentModules.Except(oldModules, pathComparer);
            var deletedModules = oldModules.Except(currentModules, pathComparer);
            var changedModules = oldModules.Zip(currentModules, (old, current) => new { old, current })
                .Where(x => pathComparer.Equals(x.old, x.current))
                .Where(x => x.old.Equals(x.current) == false)
                .Select(c => c.current);

            var added = addedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Added));
            var deleted = deletedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Deleted));
            var changed = changedModules.Select(m => new ModuleDifference(m, ModuleDifferenceType.Changed));
            return added.Concat(deleted).Concat(changed).ToArray();
        }

        class ModulePathComparer : IEqualityComparer<Module>
        {
            public bool Equals(Module x, Module y)
            {
                return x.Path == y.Path;
            }

            public int GetHashCode(Module module)
            {
                return module.Path.GetHashCode();
            }
        }

    }
}
