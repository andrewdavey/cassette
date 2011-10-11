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

using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    public class BundleDescriptor
    {
        readonly IFile sourceFile;
        readonly IEnumerable<string> assetFilenames;
        readonly bool assetsSorted;
        readonly IEnumerable<string> references;
        readonly string externalUrl;
        readonly string fallbackCondition;

        public BundleDescriptor(IEnumerable<string> assetFilenames)
        {
            this.assetFilenames = assetFilenames;
            references = Enumerable.Empty<string>();
        }

        public BundleDescriptor(IFile sourceFile, IEnumerable<string> assetFilenames, bool assetsSorted, IEnumerable<string> references, string externalUrl, string fallbackCondition)
        {
            this.sourceFile = sourceFile;
            this.assetFilenames = assetFilenames;
            this.assetsSorted = assetsSorted;
            this.references = references;
            this.externalUrl = externalUrl;
            this.fallbackCondition = fallbackCondition;
        }

        public IFile SourceFile
        {
            get { return sourceFile; }
        }

        public IEnumerable<string> AssetFilenames
        {
            get { return assetFilenames; }
        }

        public bool AssetsSorted
        {
            get { return assetsSorted; }
        }

        public IEnumerable<string> References
        {
            get { return references; }
        }

        public string ExternalUrl
        {
            get { return externalUrl; }
        }

        public string FallbackCondition
        {
            get { return fallbackCondition; }
        }
    }
}

