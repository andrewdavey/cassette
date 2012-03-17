using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class FileAsset : AssetBase
    {
        public FileAsset(IFile sourceFile, Bundle parentBundle)
        {
            this.parentBundle = parentBundle;
            this.sourceFile = sourceFile;

            hash = new Lazy<byte[]>(ComputeHash); // Avoid file IO until the hash is actually needed.
        }

        readonly Bundle parentBundle;
        readonly IFile sourceFile;
        readonly Lazy<byte[]> hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override string Path
        {
            get { return sourceFile.FullPath; }
        }

        public override byte[] Hash
        {
            get { return hash.Value; }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public override void AddReference(string assetRelativePath, int lineNumber)
        {
            if (assetRelativePath.IsUrl())
            {
                AddUrlReference(assetRelativePath, lineNumber);
            }
            else
            {
                string appRelativeFilename;
                if (assetRelativePath.StartsWith("~"))
                {
                    appRelativeFilename = assetRelativePath;
                }
                else if (assetRelativePath.StartsWith("/"))
                {
                    appRelativeFilename = "~" + assetRelativePath;
                }
                else
                {
                    var subDirectory = sourceFile.Directory.FullPath;
                    appRelativeFilename = PathUtilities.CombineWithForwardSlashes(
                        subDirectory,
                        assetRelativePath
                        );
                }
                appRelativeFilename = PathUtilities.NormalizePath(appRelativeFilename);
                AddBundleReference(appRelativeFilename, lineNumber);
            }
        }

        void AddBundleReference(string appRelativeFilename, int lineNumber)
        {
            var type = parentBundle.ContainsPath(appRelativeFilename)
                           ? AssetReferenceType.SameBundle
                           : AssetReferenceType.DifferentBundle;
            references.Add(new AssetReference(appRelativeFilename, this, lineNumber, type));
        }

        void AddUrlReference(string url, int sourceLineNumber)
        {
            references.Add(new AssetReference(url, this, sourceLineNumber, AssetReferenceType.Url));
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            if (relativeFilename.StartsWith("/"))
            {
                relativeFilename = "~" + relativeFilename;
            }
            else if (!relativeFilename.StartsWith("~"))
            {
                relativeFilename = PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(
                    sourceFile.Directory.FullPath,
                    relativeFilename
                ));
            }

            var alreadyExists = references.Any(r => r.Path.Equals(relativeFilename, StringComparison.OrdinalIgnoreCase));
            if (alreadyExists) return;

            references.Add(new AssetReference(relativeFilename, this, -1, AssetReferenceType.RawFilename));
        }

        byte[] ComputeHash()
        {
            using (var sha1 = SHA1.Create())
            using (var stream = OpenStream())
            {
                return sha1.ComputeHash(stream);
            }
        }

        protected override Stream OpenStreamCore()
        {
            return sourceFile.OpenRead();
        }

        public override void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}