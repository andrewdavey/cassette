using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cassette.RequireJS
{
    class StringAsset : AssetBase
    {
        readonly string path;
        readonly Lazy<string> content;

        public StringAsset(string path, Lazy<string> content)
        {
            this.path = path;
            this.content = content;
        }

        public override string Path
        {
            get { return path; }
        }

        public override byte[] Hash
        {
            get
            {
                using (var sha1 = SHA1.Create())
                {
                    return sha1.ComputeHash(Encoding.UTF8.GetBytes(content.Value));
                }
            }
        }

        public override Type AssetCacheValidatorType
        {
            get { return typeof(AlwaysValid); }
        }

        public override IEnumerable<AssetReference> References
        {
            get { yield break; }
        }

        public override void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override Stream OpenStreamCore()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content.Value));
        }

        public override void AddReference(string path, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }
    }
}