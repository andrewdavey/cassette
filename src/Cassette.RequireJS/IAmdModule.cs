namespace Cassette.RequireJS
{
    /// <summary>
    /// Provides information about an AMD module.
    /// </summary>
    public interface IAmdModule
    {
        /// <summary>
        /// Gets the AMD path for this module.
        /// </summary>
        string ModulePath { get; }

        /// <summary>
        /// Gets a JavaScript identifier used as a function parameter by modules that import this module.
        /// </summary>
        string Alias { get; set; }
        
        /// <summary>
        /// Gets the asset that defines this module.
        /// </summary>
        IAsset Asset { get; }

        /// <summary>
        /// Gets the bundle that contains this module's asset.
        /// </summary>
        Bundle Bundle { get; }
    }
}