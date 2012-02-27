using System.Text.RegularExpressions;

namespace RequestReduce.Utilities
{
    internal class RegexCache
    {
        internal readonly Regex ImageUrlPattern = new Regex(@"\burl[\s]*\([\s]*(?<url>[^\)]*)[\s]*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        internal readonly Regex OffsetPattern = new Regex(@"background(?:-position)?:(?:[^;}]*?)(?<offset1>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;}](?:(?<offset2>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?)))?[^;}]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        internal readonly Regex RepeatPattern = new Regex(@"\b((x-)|(y-)|(no-))?repeat\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal readonly Regex WidthPattern = new Regex(@"\b(max-)?width:[\s]*(?<width>[0-9]+)(px)?[\s]*[;}]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal readonly Regex HeightPattern = new Regex(@"\b(max-)?height:[\s]*(?<height>[0-9]+)(px)?[\s]*[;}]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal readonly Regex PaddingPattern = new Regex(@"padding(?<side>-(left|top|right|bottom))?:[\s]*(?<pad1>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;}]((?<pad2>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;}])?((?<pad3>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;}])?((?<pad4>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;}])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
        internal readonly Regex CssImportPattern = new Regex(@"@import[\s]+(url[\s]*\([\s]*)?['""]?(?<url>[^'"" ]+)['""]?([\s]*\))?[\s]*(?<media>[a-zA-z][a-zA-z,\s]*[a-zA-z])?[\s]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        internal readonly Regex UrlPattern = new Regex(@"(href|src)=['""]?(?<url>[^'"" ]+)['""]?[^ />]+[ />]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal readonly Regex MediaPattern = new Regex(@"media=['""]?(?<media>[^'"" ]+)['""]?[^ />]+[ />]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal readonly Regex SelectorSplitPattern = new Regex(@"(?=[:\.\#])", RegexOptions.Compiled);
        internal readonly Regex CssCommentPattern = new Regex(@"/\*(.*?)\*/", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        internal readonly Regex PseudoElementPattern = new Regex(@":before|:after|:first-line|:first-letter", RegexOptions.Compiled);
    }
}
