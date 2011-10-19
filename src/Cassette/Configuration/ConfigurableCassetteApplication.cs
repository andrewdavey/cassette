using System;
using System.Collections.Generic;
using Cassette.HtmlTemplates;
using Cassette.Persistence;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Configuration
{
    public class ConfigurableCassetteApplication : IConfigurableCassetteApplication
    {
        public ConfigurableCassetteApplication()
        {
            bundles = new BundleCollection(this);
            services = new CassetteServices();
            settings = new CassetteSettings();
            bundleFactories = CreateBundleFactories();
        }

        readonly BundleCollection bundles;
        CassetteServices services;
        CassetteSettings settings;
        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;

        public BundleCollection Bundles
        {
            get { return bundles; }
        }

        public CassetteServices Services
        {
            get { return services; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Services cannot be null.");
                }
                services = value;
            }
        }

        public CassetteSettings Settings
        {
            get { return settings; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Settings cannot be null.");
                }
                settings = value;
            }
        }

        // TODO: Maybe expose this to allow consumers to provide their own Bundle types?
        internal IDictionary<Type,IBundleFactory<Bundle>> BundleFactories
        {
            get { return bundleFactories; }
        }

        internal IBundleContainer CreateBundleContainer(ICassetteApplication application, string cacheVersion)
        {
            IBundleContainerFactory containerFactory;
            if (settings.IsDebuggingEnabled)
            {
                containerFactory = new BundleContainerFactory(bundleFactories);
            }
            else
            {
                containerFactory = new CachedBundleContainerFactory(
                    new BundleCache(
                        cacheVersion,
                        settings.CacheDirectory,
                        settings.SourceDirectory
                    ),
                    bundleFactories
                );
            }
            return containerFactory.Create(Bundles, application);
        }

        static Dictionary<Type, IBundleFactory<Bundle>> CreateBundleFactories()
        {
            return new Dictionary<Type, IBundleFactory<Bundle>>
            {
                { typeof(ScriptBundle), new ScriptBundleFactory() },
                { typeof(StylesheetBundle), new StylesheetBundleFactory() },
                { typeof(HtmlTemplateBundle), new HtmlTemplateBundleFactory() }
            };
        }
    }
}