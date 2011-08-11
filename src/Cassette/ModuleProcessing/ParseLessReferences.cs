using System.Text.RegularExpressions;

namespace Cassette.ModuleProcessing
{
    public class ParseLessReferences : LineBasedAssetReferenceParser<Module>
    {
        public ParseLessReferences() 
            : base("less", referenceRegex)
        {
        }

        static readonly Regex referenceRegex = new Regex(
            @"// \s* @reference \s+ (?<quote>[""']) (?<path>.*?) \<quote> \s* ;?",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );
    }
}