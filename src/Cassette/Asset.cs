#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Cassette.IO;
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

            // TODO: Compute hash lazily to avoid IO when actually loading cached asset instead.
            hash = HashFileContents();
        }

        readonly string applicationRelativeFilename;
        readonly Module parentModule;
        readonly IFile file;
        readonly byte[] hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override IFile SourceFile
        {
            get { return file; }
        }

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
                    var subDirectory = Path.GetDirectoryName(applicationRelativeFilename);
                    appRelativeFilename = PathUtilities.CombineWithForwardSlashes(
                        subDirectory,
                        assetRelativeFilename
                    );
                }
                appRelativeFilename = PathUtilities.NormalizePath(appRelativeFilename);
                AddModuleReference(lineNumber, appRelativeFilename);
            }
        }

        void AddModuleReference(int lineNumber, string appRelativeFilename)
        {
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

        public override IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        byte[] HashFileContents()
        {
            using (var sha1 = SHA1.Create())
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return sha1.ComputeHash(fileStream);
            }
        }

        bool ParentModuleCouldContain(string path)
        {
            return parentModule.PathIsPrefixOf(path);
        }

        void RequireModuleContainsReference(int lineNumber, string path)
        {
            if (parentModule.ContainsPath(path)) return;
            
            throw new AssetReferenceException(
                string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    applicationRelativeFilename, lineNumber, path
                )
            );
        }

        protected override Stream OpenStreamCore()
        {
            return file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public override void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}

