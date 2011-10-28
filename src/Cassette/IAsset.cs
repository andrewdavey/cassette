#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Collections.Generic;
using System.IO;
using Cassette.IO;

namespace Cassette
{
    public interface IAsset
    {
        /// <summary>
        /// Gets the hash of the original asset contents, before any transformations are applied.
        /// </summary>
        byte[] Hash { get; }

        /// <summary>
        /// Gets the file containing the source of this asset.
        /// </summary>
        IFile SourceFile { get; }

        /// <summary>
        /// Gets the references made by this asset.
        /// </summary>
        IEnumerable<AssetReference> References { get; }
        
        // TODO: Move Accept into a separate, internal, interface?
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
