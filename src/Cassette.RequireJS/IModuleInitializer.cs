using System.Collections.Generic;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Configures script bundles to work as AMD modules.
    /// </summary>
    public interface IModuleInitializer : IEnumerable<IAmdModule>
    {
        /// <summary>
        /// Create AMD modules based on the assets in the provided bundles.
        /// </summary>
        /// <param name="bundles">The bundles to create AMD modules from.</param>
        /// <param name="requireJsScriptPath">Application relative path to the require.js script.</param>
        void InitializeModules(IEnumerable<Bundle> bundles, string requireJsScriptPath);

        /// <summary>
        /// Gets the path of the bundle that contains require.js.
        /// This should be assigned by <see cref="InitializeModules"/>.
        /// </summary>
        string MainBundlePath { get; }

        string RequireJsScriptPath { get; }

        RequireJSConfiguration RequireJsConfiguration { get; set; }

        /// <summary>
        /// Gets a module by it's path.
        /// </summary>
        IAmdModule this[string scriptPath] { get; }

        void AddRequireJsConfigAssetToMainBundle(ScriptBundle mainBundle);
    }
}