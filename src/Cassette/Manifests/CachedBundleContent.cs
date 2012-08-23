using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Configuration;

namespace Cassette.Manifests
{
    class CachedBundleContent : IAsset
    {
        readonly byte[] content;
        readonly IEnumerable<IAsset> originalAssets;

        public CachedBundleContent(byte[] content, IEnumerable<IAsset> originalAssets, CassetteSettings settings)
        {
            this.content = settings.IsUsingPrecompiledManifest ? TransformUrls(content, settings.UrlModifier) : content;
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
                    match => urlModifier.PreCacheModify(match.Groups[1].Value)
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

        public IFile SourceFile
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