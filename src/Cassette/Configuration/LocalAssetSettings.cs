namespace Cassette.Configuration
{
    /// <summary>
    /// The local assets settings for an external bundle.
    /// </summary>
    public class LocalAssetSettings
    {
        public LocalAssetSettings()
        {
            Path = "~/";
            UseWhenDebugging = true;
        }

        /// <summary>
        /// Gets or sets the application relative path to the local assets.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Gets or sets if the local assets should be used when application is in debug mode. Default is true.
        /// </summary>
        public bool UseWhenDebugging { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="IFileSearch"/> used to find asset files. If null the bundle type's application default <see cref="IFileSearch"/> will be used.
        /// </summary>
        public IFileSearch FileSearch { get; set; }
        /// <summary>
        /// Gets or sets a JavaScript fallback condition. Used to load the local assets when the remote asset has failed to load.
        /// </summary>
        public string FallbackCondition { get; set; }
    }
}