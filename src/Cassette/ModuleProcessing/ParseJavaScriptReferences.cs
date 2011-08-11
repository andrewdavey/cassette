using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.ModuleProcessing
{
    public class ParseJavaScriptReferences : LineBasedAssetReferenceParser<Module>
    {
        public ParseJavaScriptReferences() : base("js", referenceRegex) { }

        static readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* (?<quote>[""']) (?<path>.*?) \<quote> \s* /?>",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );
    }
}
