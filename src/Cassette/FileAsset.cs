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

            hash = new Lazy<byte[]>(HashFileContents); // Avoid file IO until the hash is actually needed.
        }

        readonly Bundle parentBundle;
        readonly IFile sourceFile;
        readonly Lazy<byte[]> hash;
        readonly List<AssetReference> references = new List<AssetReference>();

        public override IFile SourceFile
        {
            get { return sourceFile; }
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
                    var subDirectory = SourceFile.Directory.FullPath;
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
            AssetReferenceType type;
            if (ParentBundleCouldContain(appRelativeFilename))
            {
                RequireBundleContainsReference(lineNumber, appRelativeFilename);
                type = AssetReferenceType.SameBundle;
            }
            else
            {
                type = AssetReferenceType.DifferentBundle;
            }
            references.Add(new AssetReference(appRelativeFilename, this, lineNumber, type));
        }

        void AddUrlReference(string url, int sourceLineNumber)
        {
            references.Add(new AssetReference(url, this, sourceLineNumber, AssetReferenceType.Url));
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            var appRelativeFilename = relativeFilename.StartsWith("~")
                ? relativeFilename 
                : PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(
                    SourceFile.Directory.FullPath,
                    relativeFilename
                ));
            
            var alreadyExists = references.Any(r => r.Path.Equals(appRelativeFilename, StringComparison.OrdinalIgnoreCase));
            if (alreadyExists) return;

            references.Add(new AssetReference(appRelativeFilename, this, -1, AssetReferenceType.RawFilename));
        }

        byte[] HashFileContents()
        {
            using (var sha1 = SHA1.Create())
            using (var fileStream = sourceFile.OpenRead())
            {
                return sha1.ComputeHash(fileStream);
            }
        }

        bool ParentBundleCouldContain(string path)
        {
            if (path.Length < parentBundle.Path.Length) return false;
            var prefix = path.Substring(0, parentBundle.Path.Length);
            return PathUtilities.PathsEqual(prefix, parentBundle.Path);
        }

        void RequireBundleContainsReference(int lineNumber, string path)
        {
            if (parentBundle.ContainsPath(path)) return;
            
            throw new AssetReferenceException(
                string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    SourceFile.FullPath, lineNumber, path
                )
            );
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

