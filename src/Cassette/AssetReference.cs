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
using System.Xml.Linq;

namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string path, IAsset sourceAsset, int sourceLineNumber, AssetReferenceType type)
        {
            if (type != AssetReferenceType.Url && path.StartsWith("~") == false)
            {
                throw new ArgumentException("Referenced path must be application relative and start with a \"~\".");
            }
            Path = path;
            SourceAsset = sourceAsset;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        /// <summary>
        /// Path to an asset or bundle.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The type of reference.
        /// </summary>
        public AssetReferenceType Type { get; set; }

        /// <summary>
        /// The asset that made this reference.
        /// </summary>
        public IAsset SourceAsset { get; private set; }

        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        public int SourceLineNumber { get; private set; }

        public XElement CreateCacheManifest()
        {
            return new XElement("Reference",
                new XAttribute("Type", Enum.GetName(typeof(AssetReferenceType), Type)),
                new XAttribute("Path", Path),
                new XAttribute("SourceLineNumber", SourceLineNumber)
            );
        }
    }
}
