using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;

namespace Cassette
{
    public class Asset : IAsset
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
        readonly List<IAssetTransformer> transformers = new List<IAssetTransformer>();
        readonly List<AssetReference> references = new List<AssetReference>();

        public void AddReference(string filename)
        {
            var absoluteFilename = PathUtilities.NormalizePath(
                Path.GetDirectoryName(this.filename),
                filename
            );
            var type = ModuleCouldContain(absoluteFilename) ? AssetReferenceType.SameModule : AssetReferenceType.DifferentModule;
            references.Add(new AssetReference(absoluteFilename, type));
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            transformers.Add(transformer);
        }

        public Stream OpenStream()
        {
            // Passing an already created stream to the transformers would make deciding who has to 
            // close the stream confusing. Use a Func<Stream> instead allows a transformer to 
            // choose when to create the stream and also then close it.
            Func<Stream> createStream = () => File.OpenRead(filename);
            foreach (var transformer in transformers)
            {
                createStream = transformer.Transform(createStream, this);
            }
            return createStream();
        }

        public string SourceFilename
        {
            get { return filename; }
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

        bool ModuleCouldContain(string path)
        {
            return path.StartsWith(parentModule.Directory, StringComparison.OrdinalIgnoreCase);
        }
    }
}
