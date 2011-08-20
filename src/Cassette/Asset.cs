using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.Utilities;

namespace Cassette
{
    public class Asset : AssetBase
    {
        public Asset(string moduleRelativeFilename, Module parentModule, IFileSystem moduleDirectory)
        {
            if (moduleRelativeFilename == null)
            {
                throw new ArgumentNullException("moduleRelativeFilename");
            }
            if (Path.IsPathRooted(moduleRelativeFilename))
            {
                throw new ArgumentException("Asset filename must be relative to it's module directory.");
            }

            this.moduleRelativeFilename = moduleRelativeFilename;
            filename = Path.GetFileName(moduleRelativeFilename);
            this.parentModule = parentModule;
            directory = moduleDirectory.NavigateTo(Path.GetDirectoryName(moduleRelativeFilename), false);
            hash = HashFileContents();
        }

        readonly string filename;
        readonly string moduleRelativeFilename;
        readonly Module parentModule;
        readonly IFileSystem directory;
        readonly byte[] hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override void AddReference(string assetRelativeFilename, int lineNumber)
        {
            if (assetRelativeFilename.StartsWith("~"))
            {
                assetRelativeFilename = assetRelativeFilename.Substring(1);
                // So the path now starts with "/".
            }

            // If filename starts with "/" then Path.Combine will ignore the parentModule.Path.
            // NormalizePath ignores this starting slash, so we get back a nice application relative path.

            var appRelativeFilename = PathUtilities.NormalizePath(Path.Combine(
                parentModule.Path,
                Path.GetDirectoryName(moduleRelativeFilename),
                assetRelativeFilename
            ));
            AssetReferenceType type;
            if (ModuleCouldContain(appRelativeFilename))
            {
                RequireModuleContainsReference(lineNumber, appRelativeFilename);
                type = AssetReferenceType.SameModule;
            }
            else
            {
                type = AssetReferenceType.DifferentModule;
            }
            references.Add(new AssetReference(appRelativeFilename, this, lineNumber, type));
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            var appRelativeFilename = PathUtilities.NormalizePath(Path.Combine(
                parentModule.Path,
                Path.GetDirectoryName(moduleRelativeFilename),
                relativeFilename
            ));
            
            var alreadyExists = references.Any(r => r.ReferencedPath.Equals(appRelativeFilename, StringComparison.OrdinalIgnoreCase));
            if (alreadyExists) return;

            references.Add(new AssetReference(appRelativeFilename, this, -1, AssetReferenceType.RawFilename));
        }

        void RequireModuleContainsReference(int lineNumber, string assetRelativeFilename)
        {
            if (parentModule.ContainsPath(assetRelativeFilename)) return;
            
            throw new AssetReferenceException(
                string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    Path.Combine(parentModule.Path, SourceFilename), lineNumber, assetRelativeFilename
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

        byte[] HashFileContents()
        {
            using (var sha1 = SHA1.Create())
            using (var fileStream = directory.OpenFile(filename, FileMode.Open, FileAccess.Read))
            {
                return sha1.ComputeHash(fileStream);
            }
        }

        bool ModuleCouldContain(string path)
        {
            return path.StartsWith(parentModule.Path, StringComparison.OrdinalIgnoreCase);
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
