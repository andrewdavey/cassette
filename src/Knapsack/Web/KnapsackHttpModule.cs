using System.IO.IsolatedStorage;
using System.Web;
using System.Web.Hosting;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        static bool created = false;
        static readonly object createdSync = new object();
        internal static KnapsackHttpModule Instance;

        IsolatedStorageFile storage;
        ModuleCache cache;
        ModuleContainer moduleContainer;

        public ModuleContainer ModuleContainer
        {
            get { return moduleContainer; }
        }

        public ModuleCache Cache
        {
            get { return cache; }
        }

        public void Init(HttpApplication context)
        {
            lock (createdSync)
            {
                if (created) return;
                created = true;

                // Our module script files will be served from isolated storage.
                storage = IsolatedStorageFile.GetUserStoreForDomain();

                moduleContainer = BuildModuleContainer();

                cache = new ModuleCache(storage, HttpRuntime.AppDomainAppPath);
                cache.UpdateFrom(moduleContainer);

                Instance = this;

                // We need a custom virtual path provider to tell ASP.NET how
                // to load modules, which are saved in isolated storage.
                //HostingEnvironment.RegisterVirtualPathProvider(
                //    new KnapsackVirtualPathProvider(moduleContainer, cache)
                //);
            }

            context.BeginRequest += new System.EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, System.EventArgs e)
        {
            HttpContext.Current.Items["Knapsack.ReferenceBuilder"] =
                new ReferenceBuilder(Instance.moduleContainer);
        }

        ModuleContainer BuildModuleContainer()
        {
            var builder = new ModuleContainerBuilder(HttpRuntime.AppDomainAppPath);
            builder.AddModuleForEachSubdirectoryOf("scripts");
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
