using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    /// <summary>
    /// Partial implementation of IAsset that reads from an assembly resource stream.
    /// </summary>
    class ResourceAsset : IAsset
    {
        readonly string resourceName;
        readonly Assembly assembly;

        public ResourceAsset(string resourceName, Assembly assembly)
        {
            this.resourceName = resourceName;
            this.assembly = assembly;
        }

        public byte[] Hash
        {
            get
            {
                using (var stream = OpenStream())
                {
                    return stream.ComputeSHA1Hash();
                }
            }
        }

        public IFile SourceFile
        {
            get { return new ResourceFile("~/" + resourceName); }
        }

        public IEnumerable<AssetReference> References
        {
            get { yield break; }
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            // No transformations applied. Asset served as-is.
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
        }

        public void AddRawFileReference(string relativeFilename)
        {
        }

        public Stream OpenStream()
        {
            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}