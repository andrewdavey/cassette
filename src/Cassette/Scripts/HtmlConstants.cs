namespace Cassette.Scripts
{
    static class HtmlConstants
    {
        public const string ScriptHtml = "<script src=\"{0}\" type=\"text/javascript\"{1}></script>";
        public const string InlineScriptHtml = "<script type=\"text/javascript\"{0}>{1}{2}{1}</script>";
        public const string ScriptHtmlWithFallback = "<script src=\"{0}\" type=\"text/javascript\"{1}></script>{4}<script type=\"text/javascript\">{4}if({2}){{{4}{3}{4}}}{4}</script>";
    }
}