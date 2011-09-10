using Cassette;

namespace $rootnamespace$
{
    /// <summary>
    /// Configures the Cassette asset modules for the web application.
    /// </summary>
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(ModuleConfiguration moduleConfiguration, ICassetteApplication application)
        {
            // TODO: Configure your asset modules here...
            // Please read http://getcassette.net/documentation/configuration

            // *** TOP TIP: Delete all ".min.js" files now ***
            // Cassette minifies scripts for you. So those files are never used.
        }
    }
}