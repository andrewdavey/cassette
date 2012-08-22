using System.Text.RegularExpressions;
using Should;

namespace Cassette
{
    public static class ShouldMatchHelper
    {
        public static void ShouldMatch(this string s, Regex regex)
        {
            regex.IsMatch(s).ShouldBeTrue(s + " did not match " + regex);
        }
    }
}