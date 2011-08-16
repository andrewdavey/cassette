using System;
using System.Linq;
using System.Collections.Generic;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;
using Cassette.ModuleProcessing;

namespace Cassette
{
    public abstract class CassetteApplicationBase : ICassetteApplication
    {
        public CassetteApplicationBase(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, bool isOutputOptmized, string version)
        {
            this.sourceFileSystem = sourceFileSystem;
            this.cacheFileSystem = cacheFileSystem;
            IsOutputOptimized = isOutputOptmized;
            this.version = CombineVersionWithCassetteVersion(version);
        }

        readonly IFileSystem sourceFileSystem;
        readonly IFileSystem cacheFileSystem;
        readonly string version;
        readonly List<Action> initializers = new List<Action>();
        readonly Dictionary<Type, IModuleContainer> moduleContainers = new Dictionary<Type, IModuleContainer>();
        
        public bool IsOutputOptimized { get; private set; }

        public IFileSystem RootDirectory
        {
            get { return sourceFileSystem; }
        }

        public string Version
        {
            get { return version; }
        }

        IModuleCache<T> GetModuleCache<T>()
            where T : Module
        {
            return new ModuleCache<T>(
                cacheFileSystem.NavigateTo(typeof(T).Name, true),
                GetModuleFactory<T>()
            );
        }

        public IReferenceBuilder<T> CreateReferenceBuilder<T>()
            where T : Module
        {
            return new ReferenceBuilder<T>(GetModuleContainer<T>(), GetModuleFactory<T>());
        }

        IModuleFactory<T> GetModuleFactory<T>()
            where T : Module
        {
            if (typeof(T) == typeof(ScriptModule))
            {
                return (IModuleFactory<T>)new ScriptModuleFactory(RootDirectory);
            }
            if (typeof(T) == typeof(StylesheetModule))
            {
                return (IModuleFactory<T>)new StylesheetModuleFactory(RootDirectory);
            }
            if (typeof(T) == typeof(HtmlTemplateModule))
            {
                return (IModuleFactory<T>)new HtmlTemplateModuleFactory(RootDirectory);
            }
            throw new NotSupportedException("Cannot find the factory for " + typeof(T).FullName + ".");
        }

        protected IModuleContainer<T> GetModuleContainer<T>()
            where T: Module
        {
            IModuleContainer container;
            if (moduleContainers.TryGetValue(typeof(T), out container))
            {
                return (IModuleContainer<T>)container;
            }
            else
            {
                return new ModuleContainer<T>(Enumerable.Empty<T>());
            }
        }

        public IAsset FindAssetByPath(string path)
        {
            foreach (var container in moduleContainers.Values)
            {
                var asset = container.FindAssetByPath(path);
                if (asset != null) return asset;
            }
            return null;
        }

        readonly Dictionary<Type, List<object>> moduleSourcesByType = new Dictionary<Type, List<object>>();

        public void Add<T>(IModuleSource<T> moduleSource)
            where T : Module
        {
            if (moduleSourcesByType.ContainsKey(typeof(T)) == false)
            {
                moduleSourcesByType[typeof(T)] = new List<object>();
                initializers.Add(() => InitializeModuleContainer<T>());
            }

            moduleSourcesByType[typeof(T)].Add(moduleSource);
        }

        void InitializeModuleContainer<T>()
            where T : Module
        {
            var results = moduleSourcesByType[typeof(T)]
                .Cast<IModuleSource<T>>()
                .Select(source => source.GetModules(GetModuleFactory<T>(), this))
                .ToArray();
            var lastWriteTimeMax = results.Max(r => r.LastWriteTimeMax);
            var modules = results.SelectMany(r => r.Modules);
            
            if (IsOutputOptimized)
            {
                var cache = GetModuleCache<T>();
                if (cache.IsUpToDate(lastWriteTimeMax, Version))
                {
                    moduleContainers[typeof(T)] = cache.LoadModuleContainer();
                }
                else
                {
                    ProcessAllModules<T>(modules);
                    var container = new ModuleContainer<T>(modules);
                    cache.SaveModuleContainer(container, Version);
                    moduleContainers[typeof(T)] = container;
                }
            }
            else
            {
                ProcessAllModules<T>(modules);
                moduleContainers[typeof(T)] = new ModuleContainer<T>(modules);
            }
        }

        void ProcessAllModules<T>(IEnumerable<T> container)
            where T : Module
        {
            foreach (var module in container)
            {
                GetPipeline<T>().Process(module, this);
            }
        }

        IModuleProcessor<T> GetPipeline<T>()
            where T : Module
        {
            return CreateDefaultPipeline<T>();
        }

        IModuleProcessor<T> CreateDefaultPipeline<T>()
            where T : Module
        {
            if (typeof(T) == typeof(StylesheetModule))
            {
                return (IModuleProcessor<T>)new StylesheetPipeline();
            }
            if (typeof(T) == typeof(ScriptModule))
            {
                return (IModuleProcessor<T>)new ScriptPipeline();
            }
            if (typeof(T) == typeof(HtmlTemplateModule))
            {
                return (IModuleProcessor<T>)new HtmlTemplatePipeline();
            }
            throw new ArgumentException("No default pipeline for module of type " + typeof(T).FullName + ".");
        }

        public void InitializeModuleContainers()
        {
            foreach (var initialize in initializers)
            {
                initialize();
            }
        }

        public abstract string CreateAbsoluteUrl(string path);
        public abstract string CreateModuleUrl(Module module);
        public abstract string CreateAssetUrl(Module module, IAsset asset);
        public abstract IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;

        /// <remarks>
        /// We need module container cache to depend on both the application version
        /// and the Cassette version. So if either is upgraded, then the cache is discarded.
        /// </remarks>
        string CombineVersionWithCassetteVersion(string version)
        {
            return version + "|" + GetType().Assembly.GetName().Version.ToString();
        }
    }
}
