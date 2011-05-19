using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Web;
using System.Web.Caching;
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

            scriptModuleContainer.UpdateStorage("scripts.xml");
            stylesheetModuleContainer.UpdateStorage("stylesheets.xml");
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
            return BuildModuleContainer(builder, config.Scripts, "scripts");
        }

        ModuleContainer BuildStylesheetModuleContainer(IsolatedStorageFile storage, KnapsackSection config)
        {
            var builder = new StylesheetModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath);
            return BuildModuleContainer(builder, config.Styles, "styles");
        }

        ModuleContainer BuildModuleContainer(ModuleContainerBuilder builder, ModuleCollection modules, string topLevelDirectoryNameConvention)
        {
            if (modules.Count == 0)
            {
                // By convention, each subdirectory of topLevelDirectoryNameConvention is a module.
                builder.AddModuleForEachSubdirectoryOf(topLevelDirectoryNameConvention);
            }
            else
            {
                AddModulesFromConfig(modules, builder);
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
                    var path = module.Path.Substring(0, module.Path.Length - 2);
                    builder.AddModuleForEachSubdirectoryOf(path);
                }
                else // the given path is the module itself.
                {
                    builder.AddModule(module.Path);
                }
            }
        }

        /// <summary>
        /// Returns a CacheDependency object that watches all module source directories for changes.
        /// </summary>
        public CacheDependency CreateCacheDependency()
        {
            var scripts = GetDirectoriesToWatch(configuration.Scripts, "scripts");
            var styles = GetDirectoriesToWatch(configuration.Styles, "styles");
            var paths = scripts.Concat(styles).ToArray();
            return new CacheDependency(paths);
        }

        IEnumerable<string> GetDirectoriesToWatch(ModuleCollection modules, string conventionalTopLevelDirectory)
        {
            var paths = new List<string>();
            if (modules.Count == 0)
            {
                // Use conventional directory e.g. "scripts".
                var scriptsPath = Path.Combine(HttpRuntime.AppDomainAppPath, conventionalTopLevelDirectory);
                if (Directory.Exists(scriptsPath))
                {
                    paths.Add(scriptsPath);
                    // HACK: CacheDependency does not seem to monitor file changes within subdirectories
                    // so manually watch each subdirectory of "scripts" as well.
                    paths.AddRange(Directory.GetDirectories(scriptsPath));
                }
            }
            else
            {
                var configPaths =
                    from element in modules.Cast<ModuleElement>()
                    select Path.Combine(HttpRuntime.AppDomainAppPath, element.Path);

                foreach (var path in configPaths)
                {
                    if (path.EndsWith("*")) // e.g. "scripts/*"
                    {
                        // So we watch all of "scripts".
                        var topLevel = path.Substring(0, path.Length - 2);
                        paths.Add(topLevel);
                        // HACK: CacheDependency does not seem to monitor file changes within subdirectories
                        // so manually watch each subdirectory of "scripts" as well.
                        paths.AddRange(Directory.GetDirectories(topLevel));
                    }
                    else
                    {
                        paths.Add(path);
                    }
                }
            }
            return paths;
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
