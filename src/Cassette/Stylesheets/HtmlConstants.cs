namespace Cassette.Stylesheets
{
    static class HtmlConstants
    {
        public const string LinkHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
        public const string LinkWithMediaHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\"/>";
        public const string ConditionalCommentStart = "<!--[if {0}]>";
        public const string ConditionalCommentEnd = "<![endif]-->";
    }
}

