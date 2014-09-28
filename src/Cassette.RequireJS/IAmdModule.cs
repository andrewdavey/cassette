﻿namespace Cassette.RequireJS
{
    /// <summary>
    /// Provides information about an AMD module.
    /// </summary>
    public interface IAmdModule
    {
        /// <summary>
        /// The AMD path for this module.
        /// </summary>
        string ModulePath { get; set; }

        /// <summary>
        /// Gets a JavaScript identifier used as a function parameter by modules that import this module.
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// The AMD module name. Used 'Alias' if it's specified, otherwise 'ModulePath' is used.
        /// </summary>
        string ModuleName { get; }
        
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