using System.Collections.Generic;

namespace Cassette.RequireJS
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
        /// Gets the asset path that defines this module.
        /// </summary>
        string Path { get; }

        IEnumerable<string> ReferencePaths { get;  }
        
        /// <summary>
        /// Gets the bundle that contains this module's asset.
        /// </summary>
        Bundle Bundle { get; }

        List<string> GetUrls(IUrlGenerator urlGenerator,bool isDebuggingEnabled);
    }
}