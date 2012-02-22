using System.Collections.Generic;

namespace Cassette
{
    public interface IReferenceBuilder
    {
        /// <summary>
        /// Adds a reference to the asset bundle with the given path.
        /// </summary>
        /// <param name="path">The application relative path to the asset bundle.</param>
        /// <param name="location">Optional page render location for the asset bundle.</param>
        void Reference(string path, string location = null);

        /// <summary>
        /// Adds a reference to the asset bundle.
        /// </summary>
        /// <param name="bundle">The asset bundle.</param>
        /// <param name="location">Optional render location for the asset bundle.</param>
        void Reference(Bundle bundle, string location = null);

        /// <summary>
        /// Returns the HTML elements that include into the page all the referenced bundles and their dependencies.
        /// </summary>
        /// <param name="location">Optional. The page location that is being rendered.</param>
        /// <returns>The HTML elements that include into the page all the referenced bundles and their dependencies</returns>
        string Render<T>(string location) where T : Bundle;

        IEnumerable<Bundle> GetBundles(string location);
    }
}