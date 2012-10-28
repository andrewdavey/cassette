using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Cassette.Spriting
{
    class SpritedCss : IAsset
    {
        readonly string css;
        readonly IAsset originalAsset;

        public SpritedCss(string css, IAsset originalAsset)
        {
            this.css = css;
            this.originalAsset = originalAsset;
            using (var sha1 = SHA1.Create())
            {
                Hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(css));
            }
        }

        public Type AssetCacheValidatorType
        {
            get { return originalAsset.AssetCacheValidatorType; }
        }

        public byte[] Hash { get; private set; }
        
        public string Path
        {
            get { return originalAsset.Path; }
        }

        public IEnumerable<AssetReference> References
        {
            get { return originalAsset.References; }
        }

        public void Accept(IBundleVisitor visitor)
        {
            originalAsset.Accept(visitor);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }

        public string GetTransformedContent()
        {
            return css;
        }
    }
}