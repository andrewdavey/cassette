using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.Utilities;

namespace Cassette.BundleProcessing
{
    class ConcatenatedAsset : AssetBase
    {
        readonly string content;
        readonly byte[] hash;
        readonly IEnumerable<IAsset> children;
        readonly string separator;

        public ConcatenatedAsset(IEnumerable<IAsset> children, string separator)
        {
            this.children = children.ToArray();
            this.separator = separator ?? Environment.NewLine;
            content = ConcatenateAssetContent(this.children);
            hash = content.ComputeSHA1Hash();
        }

        public override void Accept(IBundleVisitor visitor)
        {
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }

        public override string Path
        {
            get { throw new NotImplementedException(); }
        }

        public override byte[] Hash
        {
            get { return hash; }
        }

        public override Type AssetCacheValidatorType
        {
            get { throw new NotImplementedException(); }
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

        protected override string GetContentCore()
        {
            return content;
        }

        string ConcatenateAssetContent(IEnumerable<IAsset> assets)
        {
            var builder = new StringBuilder();

            var isFirstAsset = true;
            foreach (var asset in assets)
            {
                if (isFirstAsset)
                {
                    isFirstAsset = false;
                }
                else
                {
                    builder.Append(separator);
                }
                builder.Append(asset.GetTransformedContent());
            }

            return builder.ToString();
        }
    }
}