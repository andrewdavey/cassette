using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Knapsack.CoffeeScript;
using Knapsack.Configuration;

namespace Knapsack.Integration.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        static bool firstInit = true;
        static readonly object firstInitSync = new object();
        internal static KnapsackHttpModule Instance;

        KnapsackSection configuration;
        ModuleContainer moduleContainer;
        ICoffeeScriptCompiler coffeeScriptCompiler;
        IsolatedStorageFile storage;

        public KnapsackSection Configuration
        {
            get { return configuration; }
        }

        public ModuleContainer ModuleContainer
        {
            get { return moduleContainer; }
        }

        public ICoffeeScriptCompiler CoffeeScriptCompiler
        {
            get { return coffeeScriptCompiler; }
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

                    configuration = LoadConfigurationFromWebConfig();

                    // Module script files will be cached in isolated storage.
                    storage = IsolatedStorageFile.GetUserStoreForDomain();
                    coffeeScriptCompiler = new CoffeeScriptCompiler(path => File.ReadAllText(HttpContext.Current.Server.MapPath(path)));
                    moduleContainer = BuildModuleContainer(storage, configuration);
                    
                    moduleContainer.UpdateStorage();

                    Instance = this;
                }
            }
        }

        KnapsackSection LoadConfigurationFromWebConfig()
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration("~/web.config");
            return webConfig.Sections.OfType<KnapsackSection>().FirstOrDefault() 
                   ?? new KnapsackSection(); // Create default config is none defined.
        }

        void context_BeginRequest(object sender, System.EventArgs e)
        {
            HttpContext.Current.Items["Knapsack.ReferenceBuilder"] =
                new ReferenceBuilder(Instance.moduleContainer);
        }

        ModuleContainer BuildModuleContainer(IsolatedStorageFile storage, KnapsackSection config)
        {
            var builder = new ModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath, coffeeScriptCompiler);
            if (config.Modules.Count == 0)
            {
                builder.AddModuleForEachSubdirectoryOf("scripts");
            }
            else
            {
                foreach (ModuleElement module in config.Modules)
                {
                    if (module.Path.EndsWith("*"))
                    {
                        builder.AddModuleForEachSubdirectoryOf(module.Path.Substring(module.Path.Length - 2));
                    }
                    else
                    {
                        builder.AddModule(module.Path);
                    }
                }
            }
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
