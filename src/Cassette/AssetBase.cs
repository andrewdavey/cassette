using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette
{
    public abstract class AssetBase : IAsset
    {
        readonly List<IAssetTransformer> transformers = new List<IAssetTransformer>();

        public abstract void Accept(IBundleVisitor visitor);

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            transformers.Add(transformer);
        }

        public string GetTransformedContent()
        {
            return transformers.Aggregate(
                GetContentCore(),
                (current, transformer) => transformer.Transform(current, this)
            );
        }

        protected abstract string GetContentCore();

        public abstract string Path { get; }

        public abstract byte[] Hash { get; }

        public abstract Type AssetCacheValidatorType { get; }

        public abstract IEnumerable<AssetReference> References { get; }

        public abstract void AddReference(string path, int lineNumber);

        public abstract void AddRawFileReference(string relativeFilename);
    }
}