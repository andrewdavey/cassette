using System.Text.RegularExpressions;
using Cassette.ModuleProcessing;

namespace Cassette.Scripts
{
    public class ParseCoffeeScriptReferences : LineBasedAssetReferenceParser<Module>
    {
        public ParseCoffeeScriptReferences() : base("coffee", ReferenceRegex)
        {
        }

        static readonly Regex ReferenceRegex = new Regex(
            @"(#\s*reference\s+(?<quote>[""'])(?<path>.*?)\<quote>)"+
            "|"+
            @"(#\s*\<reference\s+path\s*=\s*(?<quote>[""'])(?<path>.*?)\<quote>\s*/?>)",
            RegexOptions.IgnoreCase
        );
    }
}
