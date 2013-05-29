using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Cassette.Caching;

namespace Cassette
{
    /// <summary>
    /// Partial implementation of IAsset that reads from an assembly resource stream.
    /// </summary>
    public class ResourceAsset : AssetBase
    {
        readonly string resourceName;
        readonly Assembly assembly;

        public ResourceAsset(string resourceName, Assembly assembly)
        {
            this.resourceName = resourceName;
            this.assembly = assembly;
        }

        public override byte[] Hash
        {
            get
            {
                using (var stream = OpenStream())
                {
                    using (var sha1 = SHA1.Create())
                    {
                        return sha1.ComputeHash(stream);
                    }
                }
            }
        }

	    public override string Path
        {
            get { return "~/" + resourceName; }
        }

        public override IEnumerable<AssetReference> References
        {
            get { yield break; }
        }

        public override void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void AddReference(string assetRelativePath, int lineNumber)
        {
        }

        public override void AddRawFileReference(string relativeFilename)
        {
        }

        protected override Stream OpenStreamCore()
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                return stream;
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

        public override Type AssetCacheValidatorType
        {
            get { return typeof(ResourceAssetCacheValidator); }
        }
    }
}