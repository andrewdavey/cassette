using System.IO.IsolatedStorage;
using System.Web;
using System.Web.Hosting;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        IsolatedStorageFile storage;

        public void Init(HttpApplication context)
        {
            // Our module script files will be served from isolated storage.
            storage = IsolatedStorageFile.GetUserStoreForDomain();
            
            var moduleContainer = BuildModuleContainer();
            
            var cache = new ModuleCache(storage, HttpRuntime.AppDomainAppPath);
            cache.UpdateFrom(moduleContainer);

            // We need a custom virtual path provider to tell ASP.NET how
            // to load modules, which are saved in isolated storage.
            HostingEnvironment.RegisterVirtualPathProvider(
                new KnapsackVirtualPathProvider(moduleContainer, cache)
            );
        }

        ModuleContainer BuildModuleContainer()
        {
            var builder = new ModuleContainerBuilder(HttpRuntime.AppDomainAppPath);
            builder.AddModuleForEachSubdirectoryOf("Scripts");
            // TODO: expose configuration point here to allow specific modules to be added.
            return builder.Build();
        }

        public void Dispose()
        {
            if (storage != null)
            {
                storage.Dispose();
                storage = null;
            }
        }
    }
}
