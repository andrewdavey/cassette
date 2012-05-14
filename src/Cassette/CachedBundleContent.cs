using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette
{
    class CachedBundleContent : IAsset
    {
        readonly byte[] content;
        readonly IEnumerable<IAsset> originalAssets;

        public CachedBundleContent(IFile file, IEnumerable<IAsset> originalAssets, IUrlModifier urlModifier)
        {
            content = TransformUrls(file, urlModifier);
            this.originalAssets = originalAssets.ToArray();
        }

        public IEnumerable<IAsset> OriginalAssets
        {
            get { return originalAssets; }
        }

        byte[] TransformUrls(IFile file, IUrlModifier urlModifier)
        {
            using (var stream = file.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                var input = reader.ReadToEnd();
                var output = Regex.Replace(
                    input,
                    "<CASSETTE_URL_ROOT>(.*?)</CASSETTE_URL_ROOT>",
                    match => urlModifier.Modify(match.Groups[1].Value)
                );
                return Encoding.UTF8.GetBytes(output);
            }
        }

        public void Accept(IBundleVisitor visitor)
        {
            foreach (var originalAsset in originalAssets)
            {
                originalAsset.Accept(visitor);
            }
        }

        public Stream OpenStream()
        {
            if (content == null)
            {
                throw new InvalidOperationException("Cannot open stream. Bundle was created from a manifest without any content.");
            }
            return new MemoryStream(content);
        }

        public Type AssetCacheValidatorType
        {
            get { throw new NotImplementedException(); }
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<AssetReference> References
        {
            get { throw new NotImplementedException(); }
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
    }
}