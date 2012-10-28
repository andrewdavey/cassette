using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Cassette.Caching;
using Cassette.Utilities;

namespace Cassette
{
    /// <summary>
    /// Partial implementation of IAsset that reads from an assembly resource stream.
    /// </summary>
    public class ResourceAsset : IAsset
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
                return GetTransformedContent().ComputeSHA1Hash();
            }
        }

        public string Path
        {
            get { return "~/" + resourceName; }
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

        public string GetTransformedContent()
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return stream.ReadToEnd();
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Resource {0} not found in assembly {1}.",
                            resourceName,
                            assembly.FullName
                            )
                        );
                }
            }
        }

        public Type AssetCacheValidatorType
        {
            get { return typeof(ResourceAssetCacheValidator); }
        }
    }
}