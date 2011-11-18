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

using System.IO;
using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public abstract class ParseReferences<T> : IBundleProcessor<T>
        where T : Bundle
    {
        public void Process(T bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                if (ShouldParseAsset(asset))
                {
                    ParseAssetReferences(asset);
                }
            }
        }

        protected virtual bool ShouldParseAsset(IAsset asset)
        {
            return true;
        }

        void ParseAssetReferences(IAsset asset)
        {
            string code;
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                code = reader.ReadToEnd();
            }

            var commentParser = CreateCommentParser();
            var referenceParser = CreateReferenceParser(commentParser);
            var references = referenceParser.Parse(code, asset);
            foreach (var reference in references)
            {
                asset.AddReference(reference.Path, reference.LineNumber);
            }
        }

        internal virtual ReferenceParser CreateReferenceParser(ICommentParser commentParser)
        {
            return new ReferenceParser(commentParser);
        }

        internal abstract ICommentParser CreateCommentParser();
    }
}

