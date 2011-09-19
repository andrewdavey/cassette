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
using Cassette.IO;

namespace Cassette.Persistence
{
    public class CachedAssetSourceInfo : IAsset
    {
        public CachedAssetSourceInfo(string filename)
        {
            this.filename = filename;
        }

        readonly string filename;
        readonly List<AssetReference> references = new List<AssetReference>();

        public string SourceFilename
        {
            get { return filename; }
        }

        public bool HasTransformers
        {
            get { return false; }
        }

        public void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public void AddReferences(IEnumerable<AssetReference> references)
        {
            this.references.AddRange(references);
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

        public Stream OpenStream()
        {
            throw new NotImplementedException();
        }

        public IFile SourceFile
        {
            get { throw new NotImplementedException(); }
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }
    }
}

