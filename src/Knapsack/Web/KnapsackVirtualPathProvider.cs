using System.IO.IsolatedStorage;
using System.Web.Hosting;

namespace Knapsack.Web
{
    public class KnapsackVirtualPathProvider : VirtualPathProvider
    {
        readonly ModuleContainer moduleContainer;
        readonly ModuleCache moduleCache;

        public KnapsackVirtualPathProvider(ModuleContainer moduleContainer, ModuleCache moduleCache)
        {
            this.moduleContainer = moduleContainer;
            this.moduleCache = moduleCache;
        }

        public override bool FileExists(string virtualPath)
        {
            return moduleContainer.Contains(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var module = moduleContainer.FindModuleContainingScript(virtualPath);
            return new KnapsackVirtualFile(module, moduleCache);
        }
    }
}
