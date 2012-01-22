using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cassette.Configuration;

namespace Cassette
{
    /// <summary>
    /// Creates configuration objects by scanning assemblies for classes that implement <see cref="ICassetteConfiguration"/>.
    /// </summary>
    class AssemblyScanningCassetteConfigurationFactory : ICassetteConfigurationFactory
    {
        readonly IEnumerable<Assembly> applicationAssemblies;

        public AssemblyScanningCassetteConfigurationFactory(IEnumerable<Assembly> applicationAssemblies)
        {
            this.applicationAssemblies = applicationAssemblies;
        }

        public IEnumerable<ICassetteConfiguration> CreateCassetteConfigurations()
        {
            Trace.Source.TraceInformation("Creating CassetteConfigurations by scanning assemblies.");
            // Scan all assemblies for implementations of the interface and create instances.
            return from assembly in applicationAssemblies
                   from type in GetConfigurationTypes(assembly)
                   select CreateConfigurationInstance(type);
        }

        IEnumerable<Type> GetConfigurationTypes(Assembly assembly)
        {
            return GetLoadableTypesFromAssembly(assembly)
                .Where(IsCassetteConfigurationType);
        }

        IEnumerable<Type> GetLoadableTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                // Some types failed to load, often due to a referenced assembly being missing.
                // This is not usually a problem, so just continue with whatever did load.
                return exception.Types.Where(type => type != null);
            }
        }

        bool IsCassetteConfigurationType(Type type)
        {
            return type.IsPublic
                   && type.IsClass
                   && !type.IsAbstract
                   && typeof(ICassetteConfiguration).IsAssignableFrom(type);
        }

        ICassetteConfiguration CreateConfigurationInstance(Type type)
        {
            Trace.Source.TraceInformation("Creating {0}.", type.FullName);
            return (ICassetteConfiguration)Activator.CreateInstance(type);
        }
    }
}
