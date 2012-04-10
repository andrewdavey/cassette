#if !NET35
using System.Web.Mvc;

namespace Cassette.Web
{
    public static class UrlHelperExtensionMethods
    {
        public static IUrlHelperExtensions Implementation { get; set; }

        /// <summary>
        /// Returns the Cassette cache-friendly URL for a file, such as an image.
        /// </summary>
        /// <param name="urlHelper">The page UrlHelper object.</param>
        /// <param name="applicationRelativeFilePath">The application relative file path of the file.</param>
        public static string CassetteFile(this UrlHelper urlHelper, string applicationRelativeFilePath)
        {
            return Implementation.CassetteFile(applicationRelativeFilePath);
        }
    }
}
#endif