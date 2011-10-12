namespace Cassette.Configuration
{
    /// <summary>
    /// Configures the Cassette application object, by adding bundles and modifying settings.
    /// </summary>
    public interface ICassetteConfiguration
    {
        /// <summary>
        /// Configures the Cassette application object, by adding bundles and modifying settings.
        /// </summary>
        /// <param name="application">The application object to configure.</param>
        void Configure(IConfigurableCassetteApplication application);
    }
}