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
using System.Xml.Linq;
using Cassette.Utilities;
using Cassette.Persistence;

namespace Cassette.ModuleProcessing
{
    public class ConcatenatedAsset : AssetBase, IDisposable
    {
        readonly byte[] hash;
        readonly IEnumerable<IAsset> children;
        readonly MemoryStream stream;

        public ConcatenatedAsset(IEnumerable<IAsset> children)
        {
            this.children = children.ToArray();
            stream = CopyAssetsIntoSingleStream(this.children);
            hash = stream.ComputeSHA1Hash();
        }

        public override void Accept(IAssetVisitor visitor)
        {
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }

        public override IO.IFile SourceFile
        {
            get { throw new NotImplementedException(); }
        }

        public override byte[] Hash
        {
            get { return hash; }
        }

        public override string SourceFilename
        {
            get { return string.Join(";", children.Select(c => c.SourceFilename)); }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return children.SelectMany(c => c.References); }
        }

        public override void AddReference(string path, int lineNumber)
        {
            throw new NotSupportedException();
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<XElement> CreateCacheManifest()
        {
            return children.OfType<ICacheableAsset>().SelectMany(asset => asset.CreateCacheManifest());
        }

        protected override Stream OpenStreamCore()
        {
            var newStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(newStream);
            newStream.Position = 0;
            return newStream;
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        MemoryStream CopyAssetsIntoSingleStream(IEnumerable<IAsset> assets)
        {
            var outputStream = new MemoryStream();
            var writer = new StreamWriter(outputStream);
            var isFirstAsset = true;
            foreach (var asset in assets)
            {
                if (isFirstAsset)
                {
                    isFirstAsset = false;
                }
                else
                {
                    writer.WriteLine();
                }
                WriteAsset(asset, writer);
            }

            writer.Flush();
            outputStream.Position = 0;
            return outputStream;
        }

        void WriteAsset(IAsset asset, StreamWriter writer)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                var isFirstLine = true;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                    }
                    else
                    {
                        writer.WriteLine();
                    }
                    writer.Write(line);
                }
            }
        }
    }
}

