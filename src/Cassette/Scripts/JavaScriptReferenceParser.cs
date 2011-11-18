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
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    class JavaScriptReferenceParser : ReferenceParser
    {
        static readonly Regex ReferenceRegex = new Regex(
            @"\<reference \s+ path \s* = \s* (?<quote>[""']) (?<path>.*?) \<quote> \s* /?>",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );

        public JavaScriptReferenceParser(ICommentParser commentParser)
            : base(commentParser)
        {
        }

        protected override IEnumerable<string> ParsePaths(string comment, IAsset sourceAsset, int lineNumber)
        {
            var simplePaths = base.ParsePaths(comment, sourceAsset, lineNumber);
            var xmlCommentPaths = ParseXmlDocCommentPaths(comment);
            return simplePaths.Concat(xmlCommentPaths);
        }

        static IEnumerable<string> ParseXmlDocCommentPaths(string comment)
        {
            return ReferenceRegex
                .Matches(comment)
                .Cast<Match>()
                .Select(m => m.Groups["path"].Value);
        }
    }
}
