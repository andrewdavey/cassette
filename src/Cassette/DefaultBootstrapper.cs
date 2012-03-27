using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using TinyIoC;

namespace Cassette
{
    public abstract class DefaultBootstrapperBase : IBootstrapper
    {
        TinyIoCContainer container = new TinyIoCContainer();

        public void Initialize()
        {
            var contributors = BootstrapperContributors.ToArray();

            var typeRegistrations = TypeRegistrations.Concat(contributors.SelectMany(c => c.TypeRegistrations));
            RegisterTypesAsSingletons(typeRegistrations);
            
            var collectionTypeRegistrations = CollectionTypeRegistrations.Concat(contributors.SelectMany(c => c.CollectionTypeRegistrations));
            RegisterCollectionTypes(collectionTypeRegistrations);

            var instanceRegistrations = InstanceRegistrationTypes.Concat(contributors.SelectMany(c => c.InstanceRegistrations));
            RegisterInstances(instanceRegistrations);
        }

        IEnumerable<IBootstrapperContributor> BootstrapperContributors
        {
            get
            {
                return AppDomainAssemblyTypeScanner.TypesOf<IBootstrapperContributor>()
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .Select(type => (IBootstrapperContributor)Activator.CreateInstance(type));
            }
        }

        void RegisterTypesAsSingletons(IEnumerable<TypeRegistration> typeRegistrations)
        {
            foreach (var typeRegistration in typeRegistrations)
            {
                container
                    .Register(typeRegistration.RegistrationType, typeRegistration.ImplementationType)
                    .AsSingleton();
            }
        }

        void RegisterCollectionTypes(IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                container.RegisterMultiple(collectionTypeRegistration.RegistrationType, collectionTypeRegistration.ImplementationTypes);
            }
        }

        void RegisterInstances(IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
            {
                if (instanceRegistration.Name != null)
                {
                    container.Register(instanceRegistration.RegistrationType, instanceRegistration.Instance, instanceRegistration.Name);
                }
                else
                {
                    container.Register(instanceRegistration.RegistrationType, instanceRegistration.Instance);
                }
            }
        }

        public ICassetteApplication GetApplication()
        {
            var bundleCollection = container.Resolve<BundleCollectionX>();
            foreach (var bundleDefinition in container.ResolveAll<IBundleDefinition>())
            {
                bundleDefinition.AddBundles(bundleCollection);
            }

            throw new NotImplementedException();
        }

        IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                return new[]
                {
                    new TypeRegistration(typeof(IUrlModifier), UrlModifier),
                    new TypeRegistration(typeof(IUrlGenerator), UrlGenerator),
                    new TypeRegistration(typeof(IJavaScriptMinifier), JavaScriptMinifier),
                    new TypeRegistration(typeof(IStylesheetMinifier), CssMinifier),
                    new TypeRegistration(typeof(IDefaultFileSearchProvider), DefaultFileSearchProvider),
                    new TypeRegistration(typeof(BundleCollectionX), typeof(BundleCollectionX))
                };
            }
        }

        IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                return new[]
                {
                    new CollectionTypeRegistration(typeof(IBundleDefinition), BundleDefinitions)
                };
            }
        }

        IEnumerable<InstanceRegistration> InstanceRegistrationTypes
        {
            get
            {
                return new[]
                {
                    new InstanceRegistration(typeof(CassetteSettings), CassetteSettings),
                    new InstanceRegistration(
                        typeof(Func<Type, FileSearch>),
                        new Func<Type, FileSearch>(type => container.Resolve<FileSearch>(type.Name + ".FileSearch"))
                        )
                };
            }
        }

        protected virtual Type UrlModifier
        {
            get { return typeof(VirtualDirectoryPrepender); }
        }
        
        protected abstract Type UrlGenerator { get; }

        protected virtual Type JavaScriptMinifier
        {
            get { return typeof(MicrosoftJavaScriptMinifier); }
        }

        protected virtual Type CssMinifier
        {
            get { return typeof(MicrosoftStylesheetMinifier); }
        }

        protected virtual IEnumerable<Type> BundleDefinitions
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IBundleDefinition>(); }
        }

        protected virtual CassetteSettings CassetteSettings
        {
            get { return new CassetteSettings("");}
        }

        protected virtual Type DefaultFileSearchProvider
        {
            get { return typeof(DefaultFileSearchProvider); }
        }
    }

    public class BundleCollectionX
    {
        readonly CassetteSettings settings;
        readonly IDefaultFileSearchProvider defaultFileSearchProvider;

        public BundleCollectionX(CassetteSettings settings, IDefaultFileSearchProvider defaultFileSearchProvider)
        {
            this.settings = settings;
            this.defaultFileSearchProvider = defaultFileSearchProvider;
        }

        public void Add<T>(string applicationRelativePath) where T : Bundle
        {
            var fileSearch = defaultFileSearchProvider.GetDefaultFileSearch<T>();
            var files = fileSearch.FindFiles(settings.SourceDirectory);
        }    
    }

    public interface IDefaultFileSearchProvider
    {
        IFileSearch GetDefaultFileSearch<T>() where T : Bundle;
    }

    public class DefaultFileSearchProvider : IDefaultFileSearchProvider
    {
        readonly Func<Type, FileSearch> getDefaultFileSearchForBundleType;
        readonly IEnumerable<IFileSearchModifier> fileSearchModifiers;

        public DefaultFileSearchProvider(Func<Type, FileSearch> getDefaultFileSearchForBundleType, IEnumerable<IFileSearchModifier> fileSearchModifiers)
        {
            this.getDefaultFileSearchForBundleType = getDefaultFileSearchForBundleType;
            this.fileSearchModifiers = fileSearchModifiers;
        }

        public IFileSearch GetDefaultFileSearch<T>() where T : Bundle
        {
            var fileSearch = getDefaultFileSearchForBundleType(typeof(T));
            return fileSearchModifiers.Aggregate(fileSearch, (current, modifier) => modifier.Modify(current, typeof(T)));
        }
    }

    public interface IFileSearchModifier
    {
        FileSearch Modify(FileSearch fileSearch, Type bundleType);
    }

    public interface IBundleDefinition
    {
        void AddBundles(BundleCollectionX bundles);
    }

    public interface IStylesheetMinifier : IAssetTransformer
    {
    }

    public interface IJavaScriptMinifier : IAssetTransformer
    {
    }

    public interface IBundlePipelineModifier<T> where T : Bundle
    {
        IBundlePipeline<T> Modify(IBundlePipeline<T> pipeline);
    }

    public abstract class BootstrapperContributor<T> : BootstrapperContributor
        where T : Bundle
    {
        public override IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                yield return new CollectionTypeRegistration(typeof(IFileSearchModifier), DefaultFileSearchModifiers);
                yield return new CollectionTypeRegistration(typeof(IBundlePipelineModifier<T>), BundlePipelineModifiers);
            }
        }

        public override IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                yield return new TypeRegistration(typeof(IBundlePipeline<T>), BundlePipeline);
                yield return new TypeRegistration(typeof(IBundleFactory<T>), BundleFactory);
            }
        }

        public override IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get
            {
                yield return new InstanceRegistration(typeof(FileSearch), FileSearch, typeof(T).Name + ".FileSearch");
            }
        }

        IEnumerable<Type> DefaultFileSearchModifiers
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IFileSearchModifier>(); }
        }

        IEnumerable<Type> BundlePipelineModifiers
        {
            get { return AppDomainAssemblyTypeScanner.TypesOf<IBundlePipelineModifier<T>>(); }
        }

        protected abstract Type BundlePipeline { get; }

        protected abstract Type BundleFactory { get; }

        protected abstract FileSearch FileSearch { get; }
    }

    public class ScriptBootstrapperContributor : BootstrapperContributor<ScriptBundle>
    {
        protected override Type BundlePipeline
        {
            // TODO: Rename ScriptPipeline to ScriptBundlePipeline? Same for the other bundle types...
            get { return typeof(ScriptPipeline); }
        }

        protected override Type BundleFactory
        {
            get { return typeof(ScriptBundleFactory); }
        }

        protected override FileSearch FileSearch
        {
            get
            {
                return new FileSearch
                {
                    Pattern = "*.js",
                    SearchOption = SearchOption.AllDirectories,
                    Exclude = new Regex("-vsdoc\\.js")
                };
            }
        }
    }

    public class StylesheetBootstrapperContributor : BootstrapperContributor<StylesheetBundle>
    {
        protected override Type BundlePipeline
        {
            get { return typeof(StylesheetPipeline); }
        }

        protected override Type BundleFactory
        {
            get { return typeof(StylesheetBundleFactory); }
        }

        protected override FileSearch FileSearch
        {
            get
            {
                return new FileSearch
                {
                    Pattern = "*.css",
                    SearchOption = SearchOption.AllDirectories
                };
            }
        }
    }

    public class HtmlTemplateBootstrapperContributor : BootstrapperContributor<HtmlTemplateBundle>
    {
        protected override Type BundlePipeline
        {
            get { return typeof(HtmlTemplatePipeline); }
        }

        protected override Type BundleFactory
        {
            get { return typeof(HtmlTemplateBundleFactory); }
        }

        protected override FileSearch FileSearch
        {
            get
            {
                return new FileSearch
                {
                    Pattern = "*.htm;*.html",
                    SearchOption = SearchOption.AllDirectories
                };
            }
        }
    }

    public interface IBootstrapperContributor
    {
        IEnumerable<TypeRegistration> TypeRegistrations { get; }
        IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations { get; }
        IEnumerable<InstanceRegistration> InstanceRegistrations { get; } 
    }

    public class BootstrapperContributor : IBootstrapperContributor
    {
        public virtual IEnumerable<TypeRegistration> TypeRegistrations
        {
            get { return Enumerable.Empty<TypeRegistration>(); }
        }

        public virtual IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get { return Enumerable.Empty<CollectionTypeRegistration>(); }
        }

        public virtual IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get { return Enumerable.Empty<InstanceRegistration>(); }
        }
    }

    public class TypeRegistration
    {
        public Type RegistrationType { get; private set; }
        public Type ImplementationType { get; private set; }

        public TypeRegistration(Type registrationType, Type implementationType)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException("implementationType");
            }
            if (!registrationType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException(string.Format("implementationType {0} must implement registrationType {1}.", implementationType.FullName, registrationType.FullName), "implementationType");
            }

            RegistrationType = registrationType;
            ImplementationType = implementationType;
        }
    }

    public class CollectionTypeRegistration
    {
        public Type RegistrationType { get; private set; }
        public IEnumerable<Type> ImplementationTypes { get; private set; }

        public CollectionTypeRegistration(Type registrationType, IEnumerable<Type> implementationTypes)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }
            if (implementationTypes == null)
            {
                throw new ArgumentNullException("implementationTypes");
            }
            implementationTypes = implementationTypes.ToArray();
            if (implementationTypes.Any(i => !registrationType.IsAssignableFrom(i)))
            {
                throw new ArgumentException("All implementationTypes must implement registrationType.", "implementationTypes");
            }

            RegistrationType = registrationType;
            ImplementationTypes = implementationTypes;
        }
    }

    public class InstanceRegistration
    {
        public Type RegistrationType { get; private set; }
        public object Instance { get; private set; }
        public string Name { get; private set; }

        public InstanceRegistration(Type registrationType, object instance) 
            : this(registrationType, instance, null)
        {
        }

        public InstanceRegistration(Type registrationType, object instance, string name)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (!registrationType.IsInstanceOfType(instance))
            {
                throw new ArgumentException(string.Format("Object must be an instance of {0} but was {1}.", registrationType.FullName, instance.GetType().FullName), "instance");
            }

            RegistrationType = registrationType;
            Instance = instance;
            Name = name;
        }
    }

    public abstract class FileSearchModifier<T> : IFileSearchModifier
        where T : Bundle
    {
        public FileSearch Modify(FileSearch fileSearch, Type bundleType)
        {
            return typeof(T) == bundleType 
                ? Modify(fileSearch) 
                : fileSearch;
        }

        protected abstract FileSearch Modify(FileSearch fileSearch);
    }

    // TODO: Move to Cassette.CoffeeScript
    public class CoffeeScriptFileSearchModifier : FileSearchModifier<ScriptBundle>
    {
        protected override FileSearch Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.coffee";
            return fileSearch;
        }
    }
}