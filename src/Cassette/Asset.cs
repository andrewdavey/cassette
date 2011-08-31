using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class Asset : AssetBase
    {
        public Asset(string applicationRelativeFilename, Module parentModule, IFile file)
        {
            if (applicationRelativeFilename == null)
            {
                throw new ArgumentNullException("applicationRelativeFilename");
            }
            if (applicationRelativeFilename.StartsWith("~") == false)
            {
                throw new ArgumentException("Asset filename in application relative form (starting with '~').");
            }

            this.applicationRelativeFilename = PathUtilities.NormalizePath(applicationRelativeFilename);
            this.parentModule = parentModule;
            this.file = file;
            hash = HashFileContents();
        }

        readonly string applicationRelativeFilename;
        readonly Module parentModule;
        readonly IFile file;
        readonly byte[] hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override void AddReference(string assetRelativeFilename, int lineNumber)
        {
            if (assetRelativeFilename.IsUrl())
            {
                AddUrlReference(assetRelativeFilename, lineNumber);
            }
            else
            {
                string appRelativeFilename;
                if (assetRelativeFilename.StartsWith("~"))
                {
                    appRelativeFilename = assetRelativeFilename;
                }
                else if (assetRelativeFilename.StartsWith("/"))
                {
                    appRelativeFilename = "~" + assetRelativeFilename;
                }
                else
                {
                    if (BelongsToSingletonAsset)
                    {
                        appRelativeFilename = PathUtilities.CombineWithForwardSlashes(
                            "~",
                            Path.GetDirectoryName(parentModule.Path),
                            assetRelativeFilename
                        );
                    }
                    else
                    {
                        var subDirectory = Path.GetDirectoryName(applicationRelativeFilename);
                        appRelativeFilename = PathUtilities.CombineWithForwardSlashes(
                            subDirectory,
                            assetRelativeFilename
                        );
                    }
                }
                appRelativeFilename = PathUtilities.NormalizePath(appRelativeFilename);

                AssetReferenceType type;
                if (ParentModuleCouldContain(appRelativeFilename))
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
        }

        bool BelongsToSingletonAsset
        {
            get { return applicationRelativeFilename.Length == 0; }
        }

        void AddUrlReference(string url, int sourceLineNumber)
        {
            references.Add(new AssetReference(url, this, sourceLineNumber, AssetReferenceType.Url));
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            var appRelativeFilename = PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(
                Path.GetDirectoryName(applicationRelativeFilename),
                relativeFilename
            ));
            
            var alreadyExists = references.Any(r => r.Path.Equals(appRelativeFilename, StringComparison.OrdinalIgnoreCase));
            if (alreadyExists) return;

            references.Add(new AssetReference(appRelativeFilename, this, -1, AssetReferenceType.RawFilename));
        }

        public override IEnumerable<XElement> CreateCacheManifest()
        {
            yield return new XElement("Asset",
                new XAttribute("Path", SourceFilename),
                references.Select(reference => reference.CreateCacheManifest())
            );
        }

        public override string SourceFilename
        {
            get { return applicationRelativeFilename; }
        }

        public override byte[] Hash
        {
            get { return hash; }
        }

        public override IFileSystem Directory
        {
            get { return file.Directory; }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        byte[] HashFileContents()
        {
            using (var sha1 = SHA1.Create())
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
            {
                return sha1.ComputeHash(fileStream);
            }
        }

        bool ParentModuleCouldContain(string path)
        {
            if (path.Length < parentModule.Path.Length) return false;
            path = path.Substring(0, parentModule.Path.Length);
            return PathUtilities.PathsEqual(path, parentModule.Path);
        }

        void RequireModuleContainsReference(int lineNumber, string path)
        {
            if (parentModule.ContainsPath(path)) return;
            
            throw new AssetReferenceException(
                string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    PathUtilities.CombineWithForwardSlashes(parentModule.Path, SourceFilename), lineNumber, path
                )
            );
        }

        protected override Stream OpenStreamCore()
        {
            return file.Open(FileMode.Open, FileAccess.Read);
        }

        public override void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
