using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;

namespace Cassette
{
    public class Asset : AssetBase
    {
        public Asset(string moduleRelativeFilename, Module parentModule, IFileSystem moduleDirectory)
        {
            if (Path.IsPathRooted(moduleRelativeFilename))
            {
                throw new ArgumentException("Asset filename must be relative to it's module directory.");
            }

            this.moduleRelativeFilename = moduleRelativeFilename;
            this.filename = Path.GetFileName(moduleRelativeFilename);
            this.parentModule = parentModule;
            this.directory = moduleDirectory.NavigateTo(Path.GetDirectoryName(moduleRelativeFilename), false);
            this.hash = HashFileContents(filename);
        }

        readonly string filename;
        readonly string moduleRelativeFilename;
        readonly Module parentModule;
        readonly IFileSystem directory;
        readonly byte[] hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override void AddReference(string filename, int lineNumber)
        {
            if (filename.StartsWith("~"))
            {
                filename = filename.Substring(1);
                // So the path now starts with "/".
            }

            // If filename starts with "/" then Path.Combine will ignore the parentModule.Directory.
            // NormalizePath ignores this starting slash, so we get back a nice application relative path.

            var absoluteFilename = PathUtilities.NormalizePath(Path.Combine(
                parentModule.Directory,
                Path.GetDirectoryName(this.moduleRelativeFilename),
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
            get { return moduleRelativeFilename; }
        }

        public override byte[] Hash
        {
            get { return hash; }
        }

        public override IFileSystem Directory
        {
            get { return directory; }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        byte[] HashFileContents(string filename)
        {
            using (var sha1 = SHA1.Create())
            using (var fileStream = directory.OpenFile(filename, FileMode.Open, FileAccess.Read))
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
            return directory.OpenFile(filename, FileMode.Open, FileAccess.Read);
        }

        public override void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
