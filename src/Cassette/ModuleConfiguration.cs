using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
using Cassette.Persistence;
using Cassette.Utilities;
using CreateModuleContainer = System.Func<bool, string, Cassette.ISearchableModuleContainer<Cassette.Module>>;
// CreateModuleContainer    = (useCache, applicationVersion) => ModuleContainer<ModuleType>

namespace Cassette
{
    public class ModuleConfiguration
    {
        public ModuleConfiguration(ICassetteApplication application, IDirectory cacheDirectory, Dictionary<Type, object> moduleFactories)
        {
            this.application = application;
            this.cacheDirectory = cacheDirectory;
            this.moduleFactories = moduleFactories;
        }

        readonly ICassetteApplication application;
        readonly Dictionary<Type, Tuple<object, CreateModuleContainer>> moduleSourceResultsByType = new Dictionary<Type, Tuple<object, CreateModuleContainer>>();
        readonly IDirectory cacheDirectory;
        readonly Dictionary<Type, object> moduleFactories;
        readonly Dictionary<Type, List<Action<object>>> customizations = new Dictionary<Type, List<Action<object>>>();

        public void Add<T>(params IModuleSource<T>[] moduleSources)
            where T : Module
        {
            foreach (var moduleSource in moduleSources)
            {
                Add(moduleSource);
            }
        }

        public bool ContainsModuleSources(Type moduleType)
        {
            return moduleSourceResultsByType.ContainsKey(moduleType);
        }

        void Add<T>(IModuleSource<T> moduleSource)
            where T : Module
        {
            var result = moduleSource.GetModules(GetModuleFactory<T>(), application);

            Tuple<object, CreateModuleContainer> existingTuple;
            if (moduleSourceResultsByType.TryGetValue(typeof(T), out existingTuple))
            {
                var existingResult = (IEnumerable<T>)existingTuple.Item1;
                var existingAction = existingTuple.Item2;
                // Concat the two module collections.
                // Keep the existing initialization action.
                moduleSourceResultsByType[typeof(T)] = Tuple.Create(
                    (object)existingResult.Concat(result),
                    existingAction
                );
            }
            else
            {
                moduleSourceResultsByType[typeof(T)] = Tuple.Create<object, CreateModuleContainer>(
                    result,
                    CreateModuleContainer<T>
                );
            }
        }

        public Dictionary<Type, ISearchableModuleContainer<Module>> CreateModuleContainers(bool useCache, string applicationVersion)
        {
            return moduleSourceResultsByType.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Item2(useCache, applicationVersion)
            );
        }

        IModuleContainer<T> CreateModuleContainer<T>(bool useCache, string applicationVersion)
            where T : Module
        {
            var modules = (IEnumerable<T>)moduleSourceResultsByType[typeof(T)].Item1;
            if (useCache)
            {
                return GetOrCreateCachedModuleContainer(modules, applicationVersion);
            }
            else
            {
                return CreateModuleContainer(modules);
            }
        }

        IModuleContainer<T> GetOrCreateCachedModuleContainer<T>(IEnumerable<T> modules, string applicationVersion) where T : Module
        {
            var cache = GetModuleCache<T>();
            IModuleContainer<T> container;
            if (cache.LoadContainerIfUpToDate(modules, applicationVersion, application.RootDirectory, out container))
            {
                return container;
            }
            else
            {
                container = CreateModuleContainer(modules);
                return cache.SaveModuleContainer(container.Modules, applicationVersion);
            }
        }

        ModuleContainer<T> CreateModuleContainer<T>(IEnumerable<T> modules) where T : Module
        {
            var modulesArray = modules.ToArray();
            List<Action<object>> customizeActions;
            if (customizations.TryGetValue(typeof(T), out customizeActions))
            {
                foreach (var customize in customizeActions)
                {
                    foreach (var module in modulesArray)
                    {
                        customize(module);
                    }
                }
            }
            ProcessAll(modulesArray);
            return new ModuleContainer<T>(ConvertUrlReferencesToModules(modulesArray));
        }

        IEnumerable<T> ConvertUrlReferencesToModules<T>(IEnumerable<T> modules) where T : Module
        {
            var modulePaths = new HashSet<string>(modules.Select(m => m.Path), StringComparer.OrdinalIgnoreCase);

            foreach (var module in modules)
            {
                yield return module;

                foreach (var reference in module.References)
                {
                    if (reference.IsUrl() == false) continue;
                    if (modulePaths.Contains(reference)) continue;
                    
                    modulePaths.Add(reference);
                    yield return GetModuleFactory<T>().CreateExternalModule(reference);
                }

                var urlReferences = module.Assets
                    .SelectMany(asset => asset.References)
                    .Where(r => r.Type == AssetReferenceType.Url);

                foreach (var reference in urlReferences)
                {
                    if (modulePaths.Contains(reference.Path)) continue;

                    var urlModule = GetModuleFactory<T>().CreateExternalModule(reference.Path);
                    modulePaths.Add(urlModule.Path);
                    yield return urlModule;
                }
            }
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
                cacheDirectory.NavigateTo(typeof(T).Name, true),
                GetModuleFactory<T>()
            );
        }

        IModuleFactory<T> GetModuleFactory<T>()
            where T : Module
        {
            return (IModuleFactory<T>)moduleFactories[typeof(T)];
        }

        public void Customize<T>(Action<T> action)
            where T : Module
        {
            var list = GetOrCreateCustomizationList<T>();
            list.Add(module => action((T)module));
        }

        public void Customize<T>(Func<T, bool> predicate, Action<T> action)
            where T : Module
        {
            var list = GetOrCreateCustomizationList<T>();
            list.Add(module =>
            {
                var typedModule = (T)module;
                if (predicate(typedModule)) action(typedModule);
            });
        }

        List<Action<object>> GetOrCreateCustomizationList<T>()
            where T : Module
        {
            List<Action<object>> list;
            if (customizations.TryGetValue(typeof(T), out list) == false)
            {
                customizations[typeof(T)] = list = new List<Action<object>>();
            }
            return list;
        }
    }
}
