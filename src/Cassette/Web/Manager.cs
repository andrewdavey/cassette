using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using Cassette.CoffeeScript;
using Cassette.Configuration;

namespace Cassette.Web
{
    /// <summary>
    /// A single Manager object is created for the web application and contains all the top-level
    /// objects used by Cassette.
    /// </summary>
    public class Manager : IManager, IDisposable
    {
        readonly CassetteSection configuration;
        readonly ModuleContainer scriptModuleContainer;
        readonly ModuleContainer stylesheetModuleContainer;
        readonly ModuleContainer htmlTemplateModuleContainer;
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
            htmlTemplateModuleContainer = BuildHtmlTemplateModuleContainer(storage, configuration);

            scriptModuleContainer.UpdateStorage("scripts.xml");
            stylesheetModuleContainer.UpdateStorage("stylesheets.xml");
            htmlTemplateModuleContainer.UpdateStorage("htmlTemplates.xml");
        }

        public CassetteSection Configuration
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

        public ModuleContainer HtmlTemplateModuleContainer
        {
            get { return htmlTemplateModuleContainer; }
        }

        public ICoffeeScriptCompiler CoffeeScriptCompiler
        {
            get { return coffeeScriptCompiler; }
        }

        CassetteSection LoadConfigurationFromWebConfig()
        {
            return (CassetteSection)WebConfigurationManager.GetSection("cassette")
                   ?? new CassetteSection(); // Create default config if none defined.
        }

        ModuleContainer BuildScriptModuleContainer(IsolatedStorageFile storage, CassetteSection config)
        {
            var builder = new ScriptModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath, coffeeScriptCompiler);
            return BuildModuleContainer(builder, config.Scripts, "scripts");
        }

        ModuleContainer BuildStylesheetModuleContainer(IsolatedStorageFile storage, CassetteSection config)
        {
            var builder = new StylesheetModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath, HttpRuntime.AppDomainAppVirtualPath);
            return BuildModuleContainer(builder, config.Styles, "styles");
        }

        ModuleContainer BuildHtmlTemplateModuleContainer(IsolatedStorageFile storage, CassetteSection config)
        {
            var builder = new HtmlTemplateModuleContainerBuilder(storage, HttpRuntime.AppDomainAppPath, HttpRuntime.AppDomainAppVirtualPath);
            return BuildModuleContainer(builder, config.HtmlTemplates, "htmlTemplates");
        }

        ModuleContainer BuildModuleContainer(ModuleContainerBuilder builder, ModuleCollection modules, string topLevelDirectoryNameConvention)
        {
            if (modules.Count == 0)
            {
                // By convention, each subdirectory of topLevelDirectoryNameConvention is a module.
                builder.AddModuleForEachSubdirectoryOf(topLevelDirectoryNameConvention, "");
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
                    builder.AddModuleForEachSubdirectoryOf(path, module.Location);
                }
                else // the given path is the module itself.
                {
                    builder.AddModule(module.Path, module.Location);
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
                    let endsWithStar = element.Path.EndsWith("*")
                    select endsWithStar // Path.Combine does not like paths with a "*" in them.
                        ? Path.Combine(HttpRuntime.AppDomainAppPath, element.Path.Substring(0, element.Path.Length - 1)) + "*"
                        : Path.Combine(HttpRuntime.AppDomainAppPath, element.Path);

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
