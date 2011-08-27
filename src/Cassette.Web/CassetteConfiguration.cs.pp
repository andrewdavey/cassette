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

            // Without a configuration, Cassette uses a simple set of defaults:
            //
            // Scripts
            //    Single module from the application's root directory
            //    Files included: *.js and *.coffee
            //    Files excluded: File names ending with "-vsdoc.js"
            //    CoffeeScript compilation enabled
            //
            // Stylesheets
            //    Single module from the application's root directory
            //    Files included: *.css and *.less
            //    File excluded: none
            //    LESS compilation enabled
            //
            // HTML Templates
            //    Single module from the application's root directory
            //    Files included: *.htm and *.html
            //    Files excluded: none
            //    Compilation disabled
        }
    }
}
