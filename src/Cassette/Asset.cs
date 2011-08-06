using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;
using System.Xml.Linq;

namespace Cassette
{
    public class Asset : AssetBase
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
        readonly List<AssetReference> references = new List<AssetReference>();

        public override void AddReference(string filename, int lineNumber)
        {
            var absoluteFilename = PathUtilities.NormalizePath(
                Path.GetDirectoryName(this.filename),
                filename
            );
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

        void RequireModuleContainsReference(int lineNumber, string absoluteFilename)
        {
            if (parentModule.ContainsPath(absoluteFilename)) return;
            
            throw new AssetReferenceException(
                string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    SourceFilename, lineNumber, absoluteFilename
                )
            );
        }

        public override string SourceFilename
        {
            get { return filename; }
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
            return File.OpenRead(filename);
        }

        public override bool IsFrom(string path)
        {
            return filename.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        public override IEnumerable<XElement> CreateManifest()
        {
            yield return new XElement("asset",
                new XAttribute("filename", filename),
                new XAttribute("lastwritetime", File.GetLastWriteTimeUtc(filename).Ticks)
            );
        }
    }
}
