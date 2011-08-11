using System;
using System.Collections.Generic;
using Cassette.UI;

namespace Cassette
{
    public abstract class CassetteApplicationBase : ICassetteApplication
    {
        public CassetteApplicationBase(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, bool isOutputOptmized)
        {
            this.sourceFileSystem = sourceFileSystem;
            this.cacheFileSystem = cacheFileSystem;
            IsOutputOptimized = isOutputOptmized;
        }

        readonly IFileSystem sourceFileSystem;
        readonly IFileSystem cacheFileSystem;
        readonly List<Action> initializers = new List<Action>();
        readonly Dictionary<Type, object> moduleContainers = new Dictionary<Type, object>();

        public bool IsOutputOptimized { get; private set; }

        public IFileSystem RootDirectory
        {
            get { return sourceFileSystem; }
        }

        public IModuleCache<T> GetModuleCache<T>()
            where T : Module
        {
            return new ModuleCache<T>(
                cacheFileSystem.AtSubDirectory(typeof(T).Name, true),
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

        public abstract string CreateModuleUrl(Module module);
        public abstract string CreateAssetUrl(Module module, IAsset asset);
        public abstract IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;
    }
}
