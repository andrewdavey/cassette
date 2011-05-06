using System.IO.IsolatedStorage;
using System.Web;
using System.Web.Hosting;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        static bool firstInit = true;
        static readonly object firstInitSync = new object();
        internal static KnapsackHttpModule Instance;

        IsolatedStorageFile storage;
        ModuleContainer moduleContainer;

        public ModuleContainer ModuleContainer
        {
            get { return moduleContainer; }
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;

            if (firstInit)
            {
                lock (firstInitSync)
                {
                    if (!firstInit) return;
                    firstInit = false;

                    // Module script files will be served from isolated storage.
                    storage = IsolatedStorageFile.GetUserStoreForDomain();

                    moduleContainer = BuildModuleContainer(storage);
                    moduleContainer.UpdateStorage();

                    Instance = this;
                }
            }
        }

        void context_BeginRequest(object sender, System.EventArgs e)
        {
            HttpContext.Current.Items["Knapsack.ReferenceBuilder"] =
                new ReferenceBuilder(Instance.moduleContainer);
        }

        ModuleContainer BuildModuleContainer(IsolatedStorageFile storage)
        {
            var builder = new ModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath);
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
