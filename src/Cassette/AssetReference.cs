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

using System;
using Cassette.Utilities;

namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string path, IAsset sourceAsset, int sourceLineNumber, AssetReferenceType type)
        {
            ValidatePath(path, type);

            Path = path;
            SourceAsset = sourceAsset;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        static void ValidatePath(string path, AssetReferenceType type)
        {
            if (type == AssetReferenceType.Url)
            {
                if (!path.IsUrl())
                {
                    throw new ArgumentException("Referenced path must be a URL.", "path");
                }
            }
            else
            {
                if (!path.StartsWith("~"))
                {
                    throw new ArgumentException(
                        "Referenced path must be application relative and start with a \"~\".",
                        "path"
                    );
                }
            }
        }

        /// <summary>
        /// Path to an asset or bundle.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The type of reference.
        /// </summary>
        public AssetReferenceType Type { get; private set; }

        /// <summary>
        /// The asset that made this reference.
        /// </summary>
        public IAsset SourceAsset { get; private set; }

        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        public int SourceLineNumber { get; private set; }
    }
}
