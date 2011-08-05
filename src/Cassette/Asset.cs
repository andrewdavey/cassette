using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;

namespace Cassette
{
    public class Asset
    {
        public Asset(string filename, Module parentModule)
        {
            this.filename = filename;
            this.parentModule = parentModule;
            this.hash = HashFileContents(filename);
        }

        readonly string filename;
        readonly Module parentModule;
        readonly byte[] hash;
        readonly List<Func<Stream, Stream>> streamWrappers = new List<Func<Stream,Stream>>();
        readonly List<AssetReference> references = new List<AssetReference>();

        public void AddReference(string filename)
        {
            var absoluteFilename = PathUtilities.NormalizePath(
                Path.GetDirectoryName(this.filename),
                filename
            );
            var type = parentModule.Contains(absoluteFilename) ? AssetReferenceType.SameModule : AssetReferenceType.DifferentModule;
            references.Add(new AssetReference(absoluteFilename, type));
        }

        public void AddStreamWrapper(Func<Stream, Stream> createWrapper)
        {
            streamWrappers.Add(createWrapper);
        }

        public Stream OpenStream()
        {
            Stream stream = File.OpenRead(filename);
            foreach (var wrap in streamWrappers)
            {
                stream = wrap(stream);
            }
            return stream;   
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        byte[] HashFileContents(string filename)
        {
            using (var sha1 = SHA1.Create())
            using (var fileStream = File.OpenRead(filename))
            {
                return sha1.ComputeHash(fileStream);
            }
        }
    }
}
