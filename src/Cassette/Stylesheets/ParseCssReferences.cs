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
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ParseCssReferences : ProcessAssetsThatMatchFileExtension<StylesheetBundle>
    {
        public ParseCssReferences()
            : base("css")
        {
        }

        protected override void Process(IAsset asset, Bundle bundle)
        {
            var css = ReadAllCss(asset);
            AddReferences(css, asset);
        }

        string ReadAllCss(IAsset asset)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                return reader.ReadToEnd();
            }
        }

        void AddReferences(string css, IAsset asset)
        {
            var commentParser = new CssCommentParser();
            var referenceParser = new ReferenceParser(commentParser);
            var references = referenceParser.Parse(css, asset);
            foreach (var reference in references)
            {
                asset.AddReference(reference.Path, reference.LineNumber);
            }
        }
    }
}