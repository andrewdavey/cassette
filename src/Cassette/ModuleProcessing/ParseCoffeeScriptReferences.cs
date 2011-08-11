using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.ModuleProcessing
{
    public class ParseCoffeeScriptReferences : LineBasedAssetReferenceParser<Module>
    {
        public ParseCoffeeScriptReferences() : base("coffee", referenceRegex)
        {
        }

        static readonly Regex referenceRegex = new Regex(
            @"#\s*reference\s+(?<quote>[""'])(?<path>.*?)\<quote>",
            RegexOptions.IgnoreCase
        );
    }
}
