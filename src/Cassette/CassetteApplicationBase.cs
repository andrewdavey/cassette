using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Cassette.Assets.HtmlTemplates;
using Cassette.Assets.Scripts;
using Cassette.Assets.Stylesheets;
using Cassette.CoffeeScript;
using Cassette.Configuration;
using Cassette.ModuleBuilding;
using Cassette.Less;

namespace Cassette
{
    /// <summary>
    /// Provides the top-level Cassette implementation details use only core .NET libraries.
    /// Sub-classes will provide web framework specific code. e.g. ASP.NET
    /// </summary>
    public abstract class CassetteApplicationBase : IDisposable
    {
        readonly string appDomainAppPath;
        readonly string appDomainAppVirtualPath;
        protected readonly IsolatedStorageFile storage;
        protected readonly ICoffeeScriptCompiler coffeeScriptCompiler;
        protected readonly ILessCompiler lessCompiler;
        protected readonly CassetteSection configuration;
        protected readonly ModuleContainer scriptModuleContainer;
        protected readonly ModuleContainer stylesheetModuleContainer;
        protected readonly ModuleContainer htmlTemplateModuleContainer;

        public CassetteApplicationBase(CassetteSection configuration, string appDomainAppPath, string appDomainAppVirtualPath, IsolatedStorageFile storage)
        {
            this.configuration = configuration ?? new CassetteSection();
            this.appDomainAppPath = appDomainAppPath;
            this.appDomainAppVirtualPath = appDomainAppVirtualPath;
            this.storage = storage;
            
            coffeeScriptCompiler = new CoffeeScriptCompiler(File.ReadAllText);
            lessCompiler = new LessCompiler(File.ReadAllText);

            scriptModuleContainer = BuildScriptModuleContainer(storage, configuration);
            stylesheetModuleContainer = BuildStylesheetModuleContainer(storage, configuration);
            htmlTemplateModuleContainer = BuildHtmlTemplateModuleContainer(storage, configuration);

            scriptModuleContainer.UpdateStorage("scripts.xml");
            stylesheetModuleContainer.UpdateStorage("stylesheets.xml");
            htmlTemplateModuleContainer.UpdateStorage("htmlTemplates.xml");
        }

        protected PageAssetManager CreatePageAssetManager(bool useModules, IPlaceholderTracker placeholderTracker)
        {
            var scriptReferenceBuilder = new ReferenceBuilder(scriptModuleContainer);
            var stylesheetReferenceBuilder = new ReferenceBuilder(stylesheetModuleContainer);
            var htmlTemplateReferenceBuilder = new ReferenceBuilder(htmlTemplateModuleContainer);
            return new PageAssetManager(
                useModules,
                placeholderTracker,
                configuration,
                scriptReferenceBuilder,
                stylesheetReferenceBuilder,
                htmlTemplateReferenceBuilder
            );
        }

        protected IEnumerable<string> GetDirectoriesToWatch(ModuleCollection modules, string conventionalTopLevelDirectory)
        {
            if (modules.Count == 0)
            {
                return GetConventionalDirectoriesToWatch(conventionalTopLevelDirectory);
            }
            else
            {
                var configPaths = GetAssetModulesConfigurationPaths(modules);
                return GetDirectoriesToWatch(configPaths);
            }
        }

        IEnumerable<string> GetDirectoriesToWatch(IEnumerable<string> configPaths)
        {
            foreach (var path in configPaths)
            {
                if (path.EndsWith("*")) // e.g. "scripts/*"
                {
                    // So we watch all of "scripts".
                    var topLevel = path.Substring(0, path.Length - 2);
                    yield return topLevel;

                    // HACK: CacheDependency does not seem to monitor file changes within subdirectories
                    // so manually watch each subdirectory of "scripts" as well.
                    foreach (var subPath in Directory.GetDirectories(topLevel))
                    {
                        yield return subPath;
                    }
                }
                else
                {
                    yield return path;
                }
            }
        }

        IEnumerable<string> GetAssetModulesConfigurationPaths(ModuleCollection modules)
        {
            return
                from element in modules.Cast<ModuleElement>()
                let endsWithStar = element.Path.EndsWith("*")
                select endsWithStar // Path.Combine does not like paths with a "*" in them.
                    ? Path.Combine(appDomainAppPath, element.Path.Substring(0, element.Path.Length - 1)) + "*"
                    : Path.Combine(appDomainAppPath, element.Path);
        }

        IEnumerable<string> GetConventionalDirectoriesToWatch(string conventionalTopLevelDirectory)
        {
            // Use conventional directory e.g. "scripts".
            var assetsPath = Path.Combine(appDomainAppPath, conventionalTopLevelDirectory);
            if (!Directory.Exists(assetsPath)) return Enumerable.Empty<string>();

            var paths = new List<string>();
            paths.Add(assetsPath);
            // HACK: CacheDependency does not seem to monitor file changes within subdirectories
            // so manually watch each subdirectory of "scripts" as well.
            paths.AddRange(Directory.GetDirectories(assetsPath));
            return paths;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (storage != null)
                {
                    storage.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CassetteApplicationBase()
        {
            Dispose(false);
        }

        ModuleContainer BuildScriptModuleContainer(IsolatedStorageFile storage, CassetteSection config)
        {
            var builder = new ScriptModuleContainerBuilder(storage, appDomainAppPath, coffeeScriptCompiler);
            return BuildModuleContainer(builder, config.Scripts);
        }

        ModuleContainer BuildStylesheetModuleContainer(IsolatedStorageFile storage, CassetteSection config)
        {
            var builder = new StylesheetModuleContainerBuilder(storage, appDomainAppPath, appDomainAppVirtualPath, lessCompiler);
            return BuildModuleContainer(builder, config.Styles);
        }

        ModuleContainer BuildHtmlTemplateModuleContainer(IsolatedStorageFile storage, CassetteSection config)
        {
            var builder = new HtmlTemplateModuleContainerBuilder(storage, appDomainAppPath, appDomainAppVirtualPath);
            return BuildModuleContainer(builder, config.HtmlTemplates);
        }

        ModuleContainer BuildModuleContainer(ModuleContainerBuilder builder, ModuleCollection modules)
        {
            if (modules.Count == 0)
            {
                // By convention, the entire root directory of the web application forms a single module.
                builder.AddModule("", "");
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
    }
}
