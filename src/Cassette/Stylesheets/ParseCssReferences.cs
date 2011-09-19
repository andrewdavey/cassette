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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class ParseCssReferences : ModuleProcessorOfAssetsMatchingFileExtension<StylesheetModule>
    {
        public ParseCssReferences()
            : base("css")
        {
        }

        static readonly Regex CssCommentRegex = new Regex(
            @"/\*(?<body>.*?)\*/",
            RegexOptions.Singleline
        );
        static readonly Regex ReferenceRegex = new Regex(
            @"@reference \s+ (?<quote>[""']) (?<path>.*?) \<quote> \s* ;?",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );

        protected override void Process(IAsset asset, Module module)
        {
            var css = ReadAllCss(asset);
            foreach (var reference in ParseReferences(css))
            {
                // TODO: Add line number tracking to the parser.
                // For now use -1 as dummy line number.
                asset.AddReference(reference, -1);
            }
        }

        string ReadAllCss(IAsset asset)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                return reader.ReadToEnd();
            }
        }

        IEnumerable<string> ParseReferences(string css)
        {
            var commentBodies = CssCommentRegex
                    .Matches(css)
                    .Cast<Match>()
                    .Select(match => match.Groups["body"].Value);

            return from body in commentBodies
                   from match in ReferenceRegex.Matches(body).Cast<Match>()
                   where match.Groups["path"].Success
                   select match.Groups["path"].Value;
        }
    }
}

