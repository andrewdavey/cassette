using System;

namespace Cassette
{
    /// <summary>
    /// Prepends the virtual directory to the beginning of application relative URL paths.
    /// </summary>
    class VirtualDirectoryPrepender : IApplicationRootPrepender
    {
        readonly string virtualDirectory;

        public VirtualDirectoryPrepender(string virtualDirectory)
        {
            if (string.IsNullOrEmpty(virtualDirectory) ||
                !virtualDirectory.StartsWith("/"))
            {
                throw new ArgumentException("Virtual directory must start with a forward slash.");
            }

            if (virtualDirectory.EndsWith("/"))
            {
                this.virtualDirectory = virtualDirectory;
            }
            else
            {
                this.virtualDirectory = virtualDirectory + "/";
            }
        }

        /// <summary>
        /// Prepends the virtual directory to the beginning of the application relative URL path.
        /// </summary>
        public string Modify(string url)
        {
            return virtualDirectory + url.TrimStart('/');
        }
    }
}