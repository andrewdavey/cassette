using System.Collections.Generic;
using Cassette.Spriting.Spritastic.Parser;

namespace Cassette.Spriting.Spritastic.Selector
{
    class SelectorComparer : IComparer<BackgroundImageClass>
    {
        public int Compare(BackgroundImageClass x, BackgroundImageClass y)
        {
            if (x == y)
                return 0;
            if (x.SpecificityScore == -1)
                x.SpecificityScore = x.ScoreSpecificity();
            if (y.SpecificityScore == -1)
                y.SpecificityScore = y.ScoreSpecificity();
            var result = y.SpecificityScore.CompareTo(x.SpecificityScore);
            return result == 0 ? y.ClassOrder.CompareTo(x.ClassOrder) : result;
        }
    }
}
