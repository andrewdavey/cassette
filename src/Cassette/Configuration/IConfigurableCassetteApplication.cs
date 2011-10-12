namespace Cassette.Configuration
{
    public interface IConfigurableCassetteApplication
    {
        /// <summary>
        /// Gets the asset bundles available to the application.
        /// </summary>
        BundleCollection Bundles { get; }

        /// <summary>
        /// Gets or sets the service implementations used by Cassette.
        /// </summary>
        CassetteServices Services { get; set; }

        /// <summary>
        /// Gets or sets the settings used by Cassette.
        /// </summary>
        CassetteSettings Settings { get; set; }
    }
}