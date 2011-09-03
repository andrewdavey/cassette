using System.Collections.Generic;
using System.IO;
using Cassette.IO;

namespace Cassette
{
    public interface IAsset
    {
        /// <summary>
        /// The application relative path of the asset.
        /// </summary>
        string SourceFilename { get; }

        /// <summary>
        /// The hash of the original asset contents, before any transformations are applied.
        /// </summary>
        byte[] Hash { get; }

        IFile SourceFile { get; }
        IEnumerable<AssetReference> References { get; }

        void Accept(IAssetVisitor visitor);
        void AddAssetTransformer(IAssetTransformer transformer);
        void AddReference(string path, int lineNumber);
        void AddRawFileReference(string relativeFilename);

        /// <summary>
        /// Opens a new stream to read the transformed contents of the asset.
        /// </summary>
        /// <returns>A readable <see cref="Stream"/>.</returns>
        Stream OpenStream();
    }
}