namespace Cassette.Configuration
{
    /// <summary>
    /// Configures Cassette by adding bundles and modifying settings.
    /// </summary>
    public interface ICassetteConfiguration
    {
        /// <summary>
        /// Configures Cassette by adding bundles and modifying settings.
        /// </summary>
        /// <param name="bundles">Bundles available to the web application.</param>
        /// <param name="settings">Settings that control Cassette's behavior.</param>
        void Configure(BundleCollection bundles, CassetteSettings settings);
    }
}