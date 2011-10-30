namespace Cassette.Configuration
{
    public interface IAddUrlConfiguration
    {
        /// <summary>
        /// Defines an application relative path to use as an alias for the bundle.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path to use as an alias for the bundle.</param>
        void WithAlias(string applicationRelativePath);

        /// <summary>
        /// Configures the bundle to use local assets when in debug mode.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path to the assets.</param>
        void WithDebug(string applicationRelativePath);

        /// <summary>
        /// Configures the bundle to use local assets when in debug mode.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path to the assets.</param>
        /// <param name="fileSource">The file source used to get files.</param>
        void WithDebug(string applicationRelativePath, IFileSearch fileSource);

        /// <summary>
        /// Configures the bundle to use local assets when the URL fails to load.
        /// </summary>
        /// <param name="fallbackCondition">The JavaScript fallback condition. When true the fallback assets are used.</param>
        /// <param name="applicationRelativePath">The application relative path to the assets.</param>
        void WithFallback(string fallbackCondition, string applicationRelativePath);

        /// <summary>
        /// Configures the bundle to use local assets when the URL fails to load.
        /// </summary>
        /// <param name="fallbackCondition">The JavaScript fallback condition. When true the fallback assets are used.</param>
        /// <param name="applicationRelativePath">The application relative path to the assets.</param>
        /// <param name="fileSource">The file source used to get files.</param>
        void WithFallback(string fallbackCondition, string applicationRelativePath, IFileSearch fileSource);
    }
}