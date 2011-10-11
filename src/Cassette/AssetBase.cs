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
using Cassette.IO;
using Cassette.BundleProcessing;
using Cassette.Persistence;

namespace Cassette
{
    /// <summary>
    /// Base class for <see cref="Asset"/> and <see cref="ConcatenatedAsset"/>.
    /// </summary>
    public abstract class AssetBase : IAsset, ICacheableAsset
    {
        readonly List<IAssetTransformer> transformers = new List<IAssetTransformer>();

        public abstract void Accept(IAssetVisitor visitor);

        public bool HasTransformers
        {
            get { return transformers.Any(); }
        }

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

        public abstract IFile SourceFile { get; }

        public abstract string SourceFilename { get; }

        public abstract byte[] Hash { get; }

        public abstract IEnumerable<AssetReference> References { get; }

        public abstract void AddReference(string path, int lineNumber);

        public abstract void AddRawFileReference(string relativeFilename);

        public abstract IEnumerable<XElement> CreateCacheManifest();
    }
}

