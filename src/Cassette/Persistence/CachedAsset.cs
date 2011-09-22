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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette.Persistence
{
    public class CachedAsset : IAsset
    {
        public CachedAsset(IFile file, byte[] hash, IEnumerable<IAsset> children)
        {
            this.file = file;
            this.hash = hash;
            this.children = children.ToArray();
        }

        readonly byte[] hash;
        readonly IFile file;
        readonly IEnumerable<IAsset> children;

        public IFile SourceFile
        {
            get { return file; }
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public IEnumerable<AssetReference> References
        {
            get { return children.SelectMany(asset => asset.References); }
        }

        public bool HasTransformers
        {
            get { return false; }
        }

        public Stream OpenStream()
        {
            return file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public void Accept(IAssetVisitor visitor)
        {
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }


        public string SourceFilename
        {
            get { throw new NotImplementedException(); }
        }

        public void AddReference(string path, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }
}

