using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Knapsack.CoffeeScript;
using Knapsack.Configuration;

namespace Knapsack.Web
{
    /// <summary>
    /// A single Manager object is created for the web application and contains all the top-level
    /// objects used by Knapsack.
    /// </summary>
    public class Manager : IManager, IDisposable
    {
        readonly KnapsackSection configuration;
        readonly ModuleContainer scriptModuleContainer;
        readonly ModuleContainer stylesheetModuleContainer;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;
        readonly IsolatedStorageFile storage;

        public Manager()
        {
            configuration = LoadConfigurationFromWebConfig();

            // Module files will be cached in isolated storage.
            storage = IsolatedStorageFile.GetUserStoreForDomain();
            coffeeScriptCompiler = new CoffeeScriptCompiler(File.ReadAllText);
            scriptModuleContainer = BuildScriptModuleContainer(storage, configuration);
            stylesheetModuleContainer = BuildStylesheetModuleContainer(storage, configuration);

            scriptModuleContainer.UpdateStorage();
            stylesheetModuleContainer.UpdateStorage();
        }

        public KnapsackSection Configuration
        {
            get { return configuration; }
        }

        public ModuleContainer ScriptModuleContainer
        {
            get { return scriptModuleContainer; }
        }

        public ModuleContainer StylesheetModuleContainer
        {
            get { return stylesheetModuleContainer; }
        }

        public ICoffeeScriptCompiler CoffeeScriptCompiler
        {
            get { return coffeeScriptCompiler; }
        }

        KnapsackSection LoadConfigurationFromWebConfig()
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration("~/web.config");
            return webConfig.Sections.OfType<KnapsackSection>().FirstOrDefault()
                   ?? new KnapsackSection(); // Create default config is none defined.
        }

        ModuleContainer BuildScriptModuleContainer(IsolatedStorageFile storage, KnapsackSection config)
        {
            var builder = new ScriptModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath, coffeeScriptCompiler);
            if (config.Scripts.Count == 0)
            {
                // By convention, each subdirectory of "~/scripts" is a module.
                builder.AddModuleForEachSubdirectoryOf("scripts");
            }
            else
            {
                AddModulesFromConfig(config.Scripts, builder);
            }
            return builder.Build();
        }

        ModuleContainer BuildStylesheetModuleContainer(IsolatedStorageFile storage, KnapsackSection config)
        {
            var builder = new StylesheetModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath);
            if (config.Styles.Count == 0)
            {
                // By convention, each subdirectory of "~/styles" is a module.
                builder.AddModuleForEachSubdirectoryOf("styles");
            }
            else
            {
                AddModulesFromConfig(config.Styles, builder);
            }
            return builder.Build();
        }

        void AddModulesFromConfig(ModuleCollection moduleElements, ModuleContainerBuilder builder)
        {
            foreach (ModuleElement module in moduleElements)
            {
                // "foo/*" implies each sub-directory of "~/foo" is a module.
                if (module.Path.EndsWith("*"))
                {
                    var path = module.Path.Substring(module.Path.Length - 2);
                    builder.AddModuleForEachSubdirectoryOf(path);
                }
                else // the given path is the module itself.
                {
                    builder.AddModule(module.Path);
                }
            }
        }

        public void Dispose()
        {
            if (storage != null)
            {
                storage.Dispose();
            }
        }
    }
}
