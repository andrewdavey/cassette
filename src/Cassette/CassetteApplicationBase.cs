using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;

namespace Cassette
{
    public abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(ICassetteConfiguration configuration, IFileSystem rootDirectory, IFileSystem cacheDirectory, IUrlGenerator urlGenerator, bool isOutputOptimized, string version)
        {
            this.rootDirectory = rootDirectory;
            this.isOutputOptimized = isOutputOptimized;
            this.urlGenerator = urlGenerator;
            moduleFactories = CreateModuleFactories();
            moduleContainers = CreateModuleContainers(
                configuration,
                cacheDirectory,
                CombineVersionWithCassetteVersion(version)
            );
        }

        readonly bool isOutputOptimized;
        readonly IFileSystem rootDirectory;
        readonly IUrlGenerator urlGenerator;
        readonly Dictionary<Type, ISearchableModuleContainer<Module>> moduleContainers;
        readonly Dictionary<Type, object> moduleFactories;

        public bool IsOutputOptimized
        {
            get { return isOutputOptimized; }
        }

        public IFileSystem RootDirectory
        {
            get { return rootDirectory; }
        }

        public IUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach(var container in moduleContainers.Values)
            {
                container.Dispose();
            }
            GC.SuppressFinalize(this); 
        }

        protected IReferenceBuilder<T> CreateReferenceBuilder<T>()
            where T : Module
        {
            return new ReferenceBuilder<T>(GetModuleContainer<T>(), (IModuleFactory<T>)moduleFactories[typeof(T)]);
        }

        protected IModuleContainer<T> GetModuleContainer<T>()
            where T: Module
        {
            ISearchableModuleContainer<Module> container;
            if (moduleContainers.TryGetValue(typeof(T), out container))
            {
                return (IModuleContainer<T>)container;
            }
            else
            {
                return new ModuleContainer<T>(Enumerable.Empty<T>());
            }
        }

        public Module FindModuleContainingPath(string path)
        {
            return moduleContainers.Values
                .Select(container => container.FindModuleContainingPath(path))
                .FirstOrDefault(module => module != null);
        }

        public abstract IPageAssetManager GetPageAssetManager<T>() where T : Module;

        Dictionary<Type, ISearchableModuleContainer<Module>> CreateModuleContainers(ICassetteConfiguration config, IFileSystem cacheDirectory, string version)
        {
            var moduleConfiguration = new ModuleConfiguration(this, cacheDirectory, moduleFactories);
            config.Configure(moduleConfiguration);
            AddDefaultModuleSourcesIfEmpty(moduleConfiguration);
            return moduleConfiguration.CreateModuleContainers(isOutputOptimized, version);
        }

        Dictionary<Type, object> CreateModuleFactories()
        {
            return new Dictionary<Type, object>
            {
                { typeof(ScriptModule), new ScriptModuleFactory() },
                { typeof(StylesheetModule), new StylesheetModuleFactory() },
                { typeof(HtmlTemplateModule), new HtmlTemplateModuleFactory() }
            };
        }

        /// <remarks>
        /// We need module container cache to depend on both the application version
        /// and the Cassette version. So if either is upgraded, then the cache is discarded.
        /// </remarks>
        string CombineVersionWithCassetteVersion(string version)
        {
            return version + "|" + GetType().Assembly.GetName().Version;
        }

        void AddDefaultModuleSourcesIfEmpty(ModuleConfiguration moduleConfiguration)
        {
            if (moduleConfiguration.ContainsModuleSources(typeof(ScriptModule)) == false)
            {
                moduleConfiguration.Add(DefaultScriptModuleSource());
            }
            if (moduleConfiguration.ContainsModuleSources(typeof(StylesheetModule)) == false)
            {
                moduleConfiguration.Add(DefaultStylesheetModuleSource());
            }
            if (moduleConfiguration.ContainsModuleSources(typeof(HtmlTemplateModule)) == false)
            {
                moduleConfiguration.Add(DefaultHtmlTemplateModuleSource());
            }
        }

        DirectorySource<ScriptModule> DefaultScriptModuleSource()
        {
            return new DirectorySource<ScriptModule>("")
            {
                FilePattern = "*.js;*.coffee",
                Exclude = new Regex("-vsdoc\\.js$")
            };
        }

        DirectorySource<StylesheetModule> DefaultStylesheetModuleSource()
        {
            return new DirectorySource<StylesheetModule>("")
            {
                FilePattern = "*.css;*.less"
            };
        }

        DirectorySource<HtmlTemplateModule> DefaultHtmlTemplateModuleSource()
        {
            return new DirectorySource<HtmlTemplateModule>("")
            {
                FilePattern = "*.htm;*.html"
            };
        }
    }
}