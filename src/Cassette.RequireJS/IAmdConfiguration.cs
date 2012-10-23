using System.Collections.Generic;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Configures script bundles to work as AMD modules.
    /// </summary>
    public interface IAmdConfiguration : IEnumerable<IAmdModule>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundles">The bundles to create AMD modules from.</param>
        /// <param name="requireJsScriptPath">Application relative path to the require.js script.</param>
        void InitializeModulesFromBundles(IEnumerable<Bundle> bundles, string requireJsScriptPath);

        /// <summary>
        /// Gets the path of the bundle that contains require.js.
        /// This should be assigned by <see cref="InitializeModulesFromBundles"/>.
        /// </summary>
        string MainBundlePath { get; }

        /// <summary>
        /// Gets a module by it's path.
        /// </summary>
        IAmdModule this[string scriptPath] { get; }
    }
}