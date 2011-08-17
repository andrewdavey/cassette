using System;
using System.Collections.Generic;
using System.Linq;
// CreateModuleContainer    = (useCache, applicationVersion) => ModuleContainer<ModuleType>
using CreateModuleContainer = System.Func<bool, string, Cassette.IModuleContainer>;

namespace Cassette
{
    public class ModuleConfiguration
    {
        public ModuleConfiguration(ICassetteApplication application, IFileSystem cacheFileSystem, Dictionary<Type, object> moduleFactories)
        {
            this.application = application;
            this.cacheFileSystem = cacheFileSystem;
            this.moduleFactories = moduleFactories;
        }

        readonly ICassetteApplication application;
        readonly Dictionary<Type, Tuple<object, CreateModuleContainer>> moduleSourceResultsByType = new Dictionary<Type, Tuple<object, CreateModuleContainer>>();
        readonly IFileSystem cacheFileSystem;
        readonly Dictionary<Type, object> moduleFactories;

        public void Add<T>(IModuleSource<T> moduleSource)
            where T : Module
        {
            var result = moduleSource.GetModules(GetModuleFactory<T>(), application);

            Tuple<object, CreateModuleContainer> existingTuple;
            if (moduleSourceResultsByType.TryGetValue(typeof(T), out existingTuple))
            {
                var existingResult = (ModuleSourceResult<T>)existingTuple.Item1;
                var existingAction = existingTuple.Item2;
                // Update the existing result by merging in the new result.
                // Keep the existing initialization action.
                moduleSourceResultsByType[typeof(T)] = Tuple.Create(
                    (object)existingResult.Merge(result),
                    existingAction
                );
            }
            else
            {
                moduleSourceResultsByType[typeof(T)] = Tuple.Create<object, CreateModuleContainer>(
                    result,
                    (useCache, applicationVersion) => CreateModuleContainer<T>(useCache, applicationVersion)
                );
            }
        }

        public Dictionary<Type, IModuleContainer> CreateModuleContainers(bool useCache, string applicationVersion)
        {
            AddRootDirectorySourceForEachNonConfiguredModuleType();

            return moduleSourceResultsByType.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Item2(useCache, applicationVersion)
            );
        }

        void AddRootDirectorySourceForEachNonConfiguredModuleType()
        {
            var moduleTypesNotAdded = moduleFactories.Keys.Except(moduleSourceResultsByType.Keys);
            foreach (var type in moduleTypesNotAdded)
            {
                AddRootDirectorySourceForNonConfiguredModuleType(type);
            }
        }

        void AddRootDirectorySourceForNonConfiguredModuleType(Type moduleType)
        {
            var sourceType = typeof(DirectorySource<>).MakeGenericType(moduleType);
            var source = Activator.CreateInstance(sourceType, "");
            var addMethod = GetType().GetMethod("Add").MakeGenericMethod(moduleType);
            addMethod.Invoke(this, new[] { source });
        }

        IModuleContainer CreateModuleContainer<T>(bool useCache, string applicationVersion)
            where T : Module
        {
            var finalResult = (ModuleSourceResult<T>)moduleSourceResultsByType[typeof(T)].Item1;
            if (useCache)
            {
                return GetOrCreateCachedModuleContainer<T>(finalResult, applicationVersion);
            }
            else
            {
                return CreateModuleContainer<T>(finalResult.Modules);
            }
        }

        IModuleContainer GetOrCreateCachedModuleContainer<T>(ModuleSourceResult<T> finalResult, string applicationVersion) where T : Module
        {
            var cache = GetModuleCache<T>();
            if (cache.IsUpToDate(finalResult.LastWriteTimeMax, applicationVersion))
            {
                return cache.LoadModuleContainer();
            }
            else
            {
                var container = CreateModuleContainer<T>(finalResult.Modules);
                cache.SaveModuleContainer(container, applicationVersion);
                return container;
                // TODO: perhaps return the cached container instance instead?
                // This may be more consistent with the cache-hit scenario above.
            }
        }

        ModuleContainer<T> CreateModuleContainer<T>(IEnumerable<T> modules) where T : Module
        {
            var modulesArray = modules.ToArray();
            ProcessAll(modules);
            return new ModuleContainer<T>(modules);
        }

        void ProcessAll<T>(IEnumerable<T> modules)
            where T : Module
        {
            foreach (var module in modules)
            {
                module.Process(application);
            }
        }

        IModuleCache<T> GetModuleCache<T>()
            where T : Module
        {
            return new ModuleCache<T>(
                cacheFileSystem.NavigateTo(typeof(T).Name, true),
                GetModuleFactory<T>()
            );
        }

        IModuleFactory<T> GetModuleFactory<T>()
            where T : Module
        {
            return (IModuleFactory<T>)moduleFactories[typeof(T)];
        }
    }
}
