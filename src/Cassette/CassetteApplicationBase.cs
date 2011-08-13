using System;
using System.Collections.Generic;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;

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
        readonly Dictionary<string, ICompiler> compilers = new Dictionary<string, ICompiler>();
        
        public bool IsOutputOptimized { get; private set; }

        public IFileSystem RootDirectory
        {
            get { return sourceFileSystem; }
        }

        public string Version
        {
            get { return version; }
        }

        public IModuleCache<T> GetModuleCache<T>()
            where T : Module
        {
            return new ModuleCache<T>(
                cacheFileSystem.NavigateTo(typeof(T).Name, true),
                GetModuleFactory<T>()
            );
        }

        public virtual IModuleFactory<T> GetModuleFactory<T>()
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

        public IModuleContainer<T> GetModuleContainer<T>()
            where T: Module
        {
            // TODO: Throw better exception when module of type T is not defined.
            return (IModuleContainer<T>)moduleContainers[typeof(T)];
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

        public void AddModuleContainerFactory<T>(IModuleContainerFactory<T> moduleContainerFactory)
            where T : Module
        {
            initializers.Add(() =>
            {
                var container = moduleContainerFactory.CreateModuleContainer();
                moduleContainers[typeof(T)] = container;
            });
        }

        public void InitializeModuleContainers()
        {
            foreach (var initializer in initializers)
            {
                initializer();
            }
            initializers.Clear();
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
