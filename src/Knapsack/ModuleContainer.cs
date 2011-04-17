using System.Collections.Generic;
using System.Linq;
using System;

namespace Knapsack
{
    public class ModuleContainer
    {
        readonly Module[] modules;
        readonly Dictionary<string, Module> modulesByScriptPath;
        readonly StringComparer pathComparer = StringComparer.OrdinalIgnoreCase;
        
        public ModuleContainer(IEnumerable<Module> modules)
        {
            this.modules = modules.ToArray();

            modulesByScriptPath = (
                from module in modules
                from script in module.Scripts
                select new { script.Path, module }
            ).ToDictionary(x => x.Path, x => x.module, pathComparer);
        }

        public IEnumerable<Module> Modules
        {
            get { return modules; }
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
            return modules.Any(m => pathComparer.Equals(m.Path, modulePath));
        }

        public Module FindModule(string modulePath)
        {
            return modules.FirstOrDefault(m => pathComparer.Equals(m.Path, modulePath));
        }

        public ModuleDifference[] CompareTo(ModuleContainer oldModuleContainer)
        {
            var modulePathComparer = new ModulePathComparer(pathComparer);
            var currentModules = new HashSet<Module>(modules, modulePathComparer);
            var oldModules = new HashSet<Module>(oldModuleContainer.modules, modulePathComparer);

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
