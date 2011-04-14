using System.IO;
using System.IO.IsolatedStorage;
using System.Web.Hosting;

namespace Knapsack.Web
{
    class KnapsackVirtualFile : VirtualFile
    {
        readonly Module module;
        readonly ModuleCache moduleCache;

        public KnapsackVirtualFile(Module module, ModuleCache moduleCache)
            : base(module.Path)
        {
            this.module = module;
            this.moduleCache = moduleCache;
        }

        public override Stream Open()
        {
            return moduleCache.OpenModuleFile(module);
        }
    }
}
