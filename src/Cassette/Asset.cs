using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;

namespace Cassette
{
    public class Asset : AssetBase
    {
        public Asset(string relativeFilename, Module parentModule)
        {
            if (Path.IsPathRooted(relativeFilename))
            {
                throw new ArgumentException("Asset filename must be relative to it's module directory.");
            }

            this.relativeFilename = relativeFilename;
            this.parentModule = parentModule;
            this.hash = HashFileContents(parentModule.GetFullPath(relativeFilename));
        }

        readonly string relativeFilename;
        readonly Module parentModule;
        readonly byte[] hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override void AddReference(string filename, int lineNumber)
        {
            var absoluteFilename = PathUtilities.NormalizePath(Path.Combine(
                parentModule.Directory,
                Path.GetDirectoryName(this.relativeFilename),
                filename
            ));
            AssetReferenceType type;
            if (ModuleCouldContain(absoluteFilename))
            {
                RequireModuleContainsReference(lineNumber, absoluteFilename);
                type = AssetReferenceType.SameModule;
            }
            else
            {
                type = AssetReferenceType.DifferentModule;
            }
            references.Add(new AssetReference(absoluteFilename, this, lineNumber, type));
        }

        void RequireModuleContainsReference(int lineNumber, string filename)
        {
            if (parentModule.ContainsPath(filename)) return;
            
            throw new AssetReferenceException(
                string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    Path.Combine(parentModule.Directory, SourceFilename), lineNumber, filename
                )
            );
        }

        public override string SourceFilename
        {
            get { return relativeFilename; }
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public override IEnumerable<AssetReference> References
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

        protected override Stream OpenStreamCore()
        {
            return File.OpenRead(parentModule.GetFullPath(relativeFilename));
        }

        public override void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
