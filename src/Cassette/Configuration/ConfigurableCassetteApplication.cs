using System;

namespace Cassette.Configuration
{
    public class ConfigurableCassetteApplication : IConfigurableCassetteApplication
    {
        public ConfigurableCassetteApplication()
        {
            bundles = new BundleCollection();
            services = new CassetteServices();
            settings = new CassetteSettings();
        }

        readonly BundleCollection bundles;
        CassetteServices services;
        CassetteSettings settings;

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
    }
}