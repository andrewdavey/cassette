using System.IO.IsolatedStorage;
using System.Web.Hosting;
using System.IO;

namespace Knapsack.Web
{
    public class KnapsackVirtualPathProvider : VirtualPathProvider
    {
        readonly ModuleContainer moduleContainer;
        readonly ModuleCache moduleCache;
        readonly string pathPrefix = "~/knapsack/";

        public KnapsackVirtualPathProvider(ModuleContainer moduleContainer, ModuleCache moduleCache)
        {
            this.moduleContainer = moduleContainer;
            this.moduleCache = moduleCache;
        }

        public override string CombineVirtualPaths(string basePath, string relativePath)
        {
            return base.CombineVirtualPaths(basePath, relativePath);
        }

        public override bool FileExists(string virtualPath)
        {
            return moduleContainer.Contains(ConvertPath(virtualPath))
                || base.FileExists(virtualPath);
        }

        public override System.Web.Caching.CacheDependency GetCacheDependency(string virtualPath, System.Collections.IEnumerable virtualPathDependencies, System.DateTime utcStart)
        {
            return null;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var module = moduleContainer.FindModule(ConvertPath(virtualPath));
            if (module != null)
            {
                return new KnapsackVirtualFile(module, moduleCache);
            }
            else
            {
                return base.GetFile(virtualPath);
            }
        }

        string ConvertPath(string virtualPath)
        {
            if (virtualPath.StartsWith(pathPrefix))
            {
                return virtualPath.Substring(pathPrefix.Length).Replace(".aspx", "").Replace('/', '\\');
            }

            return virtualPath;
        }
    }
}
