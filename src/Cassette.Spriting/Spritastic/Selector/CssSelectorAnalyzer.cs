using System;
using System.Linq;
using Cassette.Spriting.Spritastic.Utilities;

namespace Cassette.Spriting.Spritastic.Selector
{
    class CssSelectorAnalyzer : ICssSelectorAnalyzer
    {
        private static readonly RegexCache Regex = new RegexCache();

        public bool IsInScopeOfTarget(string targetSelector, string comparableSelector)
        {
            var targetOffset = 0;
            var tokens = comparableSelector.Split(new char[] {' ','>'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                targetOffset = FindToken(token, targetSelector, targetOffset);
                if(targetOffset == -1)
                    return false;
            }
            return true;
        }

        private static int FindToken(string comparableSelector, string targetSelector, int targetOffset)
        {
            var tokens = Regex.SelectorSplitPattern.Split(comparableSelector);
            while (targetSelector.Length > targetOffset)
            {
                var tokenIdx = 0;
                while (tokenIdx < tokens.Length && (tokens[tokenIdx].Length == 0 || tokens[tokenIdx] == "*"))
                    ++tokenIdx;
                if (tokenIdx >= tokens.Length)
                {
                    if (tokens[tokens.Length - 1] == "*")
                        return 0;
                    return -1;
                }
                var idx = tokens[tokenIdx] == "*" ? 0 : targetSelector.IndexOf(tokens[tokenIdx], targetOffset, StringComparison.OrdinalIgnoreCase);
                if (idx == -1) return idx;
                var endIdx = idx + tokens[tokenIdx].Length;
                if ((idx == 0 || targetSelector.IndexOfAny(new[] { ' ', '\n', '\r', '\t', '>' }, idx-1, 1) == idx-1 || targetSelector[idx] == '.' || targetSelector[idx] == '#') &&
                    (targetSelector.Length <= endIdx ||
                    targetSelector.IndexOfAny(new[] {' ', '\n', '\r', '\t', ':', '.', '#', '>'}, endIdx, 1) == endIdx))
                {
                    var startTargetIdx = targetSelector.LastIndexOfAny(new[] {' ', '\n', '\r', '\t', '>'}, idx) + 1;
                    var endTargetdx = targetSelector.IndexOfAny(new[] { ' ', '\n', '\r', '\t', '>' }, idx);
                    endTargetdx = endTargetdx == -1 ? targetSelector.Length - 1 : endTargetdx - 1;
                    var targetTokens = Regex.SelectorSplitPattern.Split(targetSelector.Substring(startTargetIdx, endTargetdx - startTargetIdx + 1));
                    if (tokens.All(x => targetTokens.Contains(x, StringComparer.OrdinalIgnoreCase) || x.Length==0 || x == "*"))
                        return idx;
                }
                targetOffset = endIdx;
            }
            return -1;
        }
    }
}
