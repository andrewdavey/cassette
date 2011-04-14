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

        public ModuleContainer(Module[] modules)
        {
            this.modules = modules;

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
    }
}
