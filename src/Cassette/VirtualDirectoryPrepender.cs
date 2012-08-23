using System;

namespace Cassette
{
    /// <summary>
    /// Prepends the virtual directory to the beginning of application relative URL paths.
    /// </summary>
    class VirtualDirectoryPrepender : IUrlModifier
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
        public string PreCacheModify(string url)
        {
            return virtualDirectory + url.TrimStart('/');
        }

        /// <summary>
        /// Exists just to satisfy interface requirements. Just calls modify.
        /// </summary>
        public string PostCacheModify(string url)
        {
            return PreCacheModify(url);
        }
    }
}
