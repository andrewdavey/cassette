namespace Cassette.Configuration
{
    /// <summary>
    /// Services used by Cassette.
    /// </summary>
    public class CassetteServices
    {
        /// <summary>
        /// Gets or sets the <see cref="IUrlGenerator"/> used by Cassette to create URLs.
        /// </summary>
        public IUrlGenerator UrlGenerator { get; set; }
    }
}