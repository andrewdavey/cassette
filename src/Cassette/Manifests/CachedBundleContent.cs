using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cassette.Manifests
{
    class CachedBundleContent : IAsset
    {
        readonly byte[] content;
        readonly IEnumerable<IAsset> originalAssets;

        public CachedBundleContent(byte[] content, IEnumerable<IAsset> originalAssets, IUrlModifier urlModifier)
        {
            // TODO: Ensure blank urlModifier is passed here when loading from precompiled manifest
            this.content = TransformUrls(content, urlModifier);
            this.originalAssets = originalAssets.ToArray();
        }

        byte[] TransformUrls(byte[] bytes, IUrlModifier urlModifier)
        {
            using (var memoryStream = new MemoryStream(bytes))
            using (var reader = new StreamReader(memoryStream))
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