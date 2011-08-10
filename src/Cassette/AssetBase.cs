using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    /// <summary>
    /// Base class for <see cref="Asset"/> and <see cref="ConcatenatedAsset"/>.
    /// </summary>
    public abstract class AssetBase : IAsset
    {
        readonly List<IAssetTransformer> transformers = new List<IAssetTransformer>();

        public abstract void Accept(IAssetVisitor visitor);

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            transformers.Add(transformer);
        }

        public Stream OpenStream()
        {
            // Passing an already created stream to the transformers would make deciding who has to 
            // close the stream confusing. Using a Func<Stream> instead allows a transformer to 
            // choose when to create the stream and also then close it.
            Func<Stream> createStream = OpenStreamCore;
            foreach (var transformer in transformers)
            {
                createStream = transformer.Transform(createStream, this);
            }
            return createStream();
        }

        protected abstract Stream OpenStreamCore();

        public abstract string SourceFilename { get; }

        public virtual byte[] Hash
        {
            get { return new byte[0]; }
        }

        public abstract IEnumerable<AssetReference> References { get; }

        public abstract void AddReference(string path, int lineNumber);
    }
}
