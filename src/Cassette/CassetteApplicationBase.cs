using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;

namespace Cassette
{
    public abstract class CassetteApplicationBase : ICassetteApplication
    {
        protected CassetteApplicationBase(IEnumerable<ICassetteConfiguration> configurations, IDirectory rootDirectory, IDirectory cacheDirectory, IUrlGenerator urlGenerator, bool isOutputOptimized, string version)
        {
            this.rootDirectory = rootDirectory;
            this.isOutputOptimized = isOutputOptimized;
            this.urlGenerator = urlGenerator;
            moduleFactories = CreateModuleFactories();
            moduleContainers = CreateModuleContainers(
                configurations,
                cacheDirectory,
                CombineVersionWithCassetteVersion(version)
            );
        }

        bool isOutputOptimized;
        readonly IDirectory rootDirectory;
        IUrlGenerator urlGenerator;
        readonly Dictionary<Type, IModuleContainer<Module>> moduleContainers;
        readonly Dictionary<Type, object> moduleFactories;

        public bool IsOutputOptimized
        {
            get { return isOutputOptimized; }
            set { isOutputOptimized = value; }
        }

        public IDirectory RootDirectory
        {
            get { return rootDirectory; }
        }

        public IUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
            set
            {
                if (value == null) throw new ArgumentNullException("value", "UrlGenerator cannot be null.");
                urlGenerator = value;
            }
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

        protected IReferenceBuilder CreateReferenceBuilder<T>()
            where T : Module
        {
            return new ReferenceBuilder<T>(GetModuleContainer<T>(), (IModuleFactory<T>)moduleFactories[typeof(T)]);
        }

        protected IModuleContainer<T> GetModuleContainer<T>()
            where T: Module
        {
            IModuleContainer<Module> container;
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

        public abstract IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;

        Dictionary<Type, IModuleContainer<Module>> CreateModuleContainers(IEnumerable<ICassetteConfiguration> configurations, IDirectory cacheDirectory, string version)
        {
            var moduleConfiguration = new ModuleConfiguration(this, cacheDirectory, rootDirectory, moduleFactories, version);
            foreach (var configuration in configurations)
            {
                configuration.Configure(moduleConfiguration, this);
            }
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

        IModuleSource<ScriptModule> DefaultScriptModuleSource()
        {
            return new PerFileModuleSource<ScriptModule>("")
            {
                FilePattern = "*.js;*.coffee",
                Exclude = new Regex("-vsdoc\\.js$")
            };
        }

        IModuleSource<StylesheetModule> DefaultStylesheetModuleSource()
        {
            return new PerFileModuleSource<StylesheetModule>("")
            {
                FilePattern = "*.css;*.less"
            };
        }

        IModuleSource<HtmlTemplateModule> DefaultHtmlTemplateModuleSource()
        {
            return new PerFileModuleSource<HtmlTemplateModule>("")
            {
                FilePattern = "*.htm;*.html"
            };
        }
    }
}