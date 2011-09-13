using System.Web;

namespace Cassette.UI
{
    public interface IReferenceBuilder<T>
        where T : Module
    {
        /// <summary>
        /// Adds a reference to the asset module with the given path.
        /// </summary>
        /// <param name="path">The application relative path to the asset module.</param>
        /// <param name="location">Optional page render location for the asset module.</param>
        void Reference(string path, string location = null);

        /// <summary>
        /// Adds a reference to the asset module.
        /// </summary>
        /// <param name="module">The asset module.</param>
        /// <param name="location">Optional render location for the asset module.</param>
        void Reference(Module module, string location = null);

        /// <summary>
        /// Returns the HTML elements that include into the page all the referenced modules and their dependencies.
        /// </summary>
        /// <param name="location">Optional. The page location that is being rendered.</param>
        /// <returns>The HTML elements that include into the page all the referenced modules and their dependencies</returns>
        IHtmlString Render(string location = null);

        /// <summary>
        /// Returns the URL for the asset module with the given path.
        /// </summary>
        /// <param name="path">The application relative path to the asset module.</param>
        /// <returns>The URL for the asset module.</returns>
        string ModuleUrl(string path);
    }
}