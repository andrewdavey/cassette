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

        public Stream OpenStream()
        {
            // Passing an already created stream to the transformers would make deciding who has to 
            // close the stream confusing. Using a Func<Stream> instead allows a transformer to 
            // choose when to create the stream and also then close it.
            var createStream = transformers.Aggregate<IAssetTransformer, Func<Stream>>(
                OpenStreamCore,
                (current, transformer) => transformer.Transform(current, this)
            );
            return createStream();
        }

        protected abstract Stream OpenStreamCore();

        public abstract string Path { get; }

        public abstract byte[] Hash { get; }

        public abstract Type AssetCacheValidatorType { get; }

        public abstract IEnumerable<AssetReference> References { get; }

        public abstract void AddReference(string path, int lineNumber);

        public abstract void AddRawFileReference(string relativeFilename);
    }
}