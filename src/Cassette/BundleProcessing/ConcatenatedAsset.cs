using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.BundleProcessing
{
    class ConcatenatedAsset : AssetBase, IDisposable
    {
        readonly string path;
        readonly byte[] hash;
        readonly IEnumerable<IAsset> children;
        readonly MemoryStream stream;
        readonly string separator;

        public ConcatenatedAsset(string path, IEnumerable<IAsset> children, string separator)
        {
            this.path = path;
            this.children = children.ToArray();
            this.separator = separator ?? Environment.NewLine;
            stream = CopyAssetsIntoSingleStream(this.children);
            hash = stream.ComputeSHA1Hash();
        }

        public override void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }

        public override string Path
        {
            get { return path; }
        }

        public override byte[] Hash
        {
            get { return hash; }
        }

        public override Type AssetCacheValidatorType
        {
            get
            {
                var first = children.FirstOrDefault();
                if (first != null) return first.AssetCacheValidatorType;
                throw new InvalidOperationException();
            }
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

        protected override Stream OpenStreamCore()
        {
            return new MemoryStream(stream.ToArray());
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
                    writer.Write(separator);
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