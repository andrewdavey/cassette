using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.IO;

namespace Cassette
{
    public abstract class AssetBase : IAsset
    {
        private byte[] postProcessingByteArray = null;

        public string postProcessingString
        {
            get { return Encoding.UTF8.GetString(postProcessingByteArray); }
        }

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
            if (postProcessingByteArray != null)
            { 
                return new MemoryStream(postProcessingByteArray);
            }
            var createStream = transformers.Aggregate<IAssetTransformer, Func<Stream>>(
                OpenStreamCore,
                (current, transformer) => transformer.Transform(current, this)
            );
            return createStream();
        }

        public void PreparePostProcessingStream()
        {
            if (postProcessingByteArray != null)
            {
                return;
            }
            var postProcessingStream = transformers.Aggregate<IAssetTransformer, Func<Stream>>(
                OpenStreamCore,
                (current, transformer) => transformer.Transform(current, this))();
            postProcessingByteArray = new byte[postProcessingStream.Length];
            postProcessingStream.Read(postProcessingByteArray, 0, (int) postProcessingStream.Length);
        }

        protected abstract Stream OpenStreamCore();

        public abstract IFile SourceFile { get; }

        public abstract byte[] Hash { get; }

        public abstract IEnumerable<AssetReference> References { get; }

        public abstract void AddReference(string path, int lineNumber);

        public abstract void AddRawFileReference(string relativeFilename);
    }
}

