namespace Cassette.Scripts
{
    class HtmlConstants
    {
        public static readonly string ScriptHtml = "<script src=\"{0}\" type=\"text/javascript\"></script>";
        public static readonly string FallbackScriptHtml = "<script type=\"text/javascript\">{0} && document.write(unescape('%3Cscript src=\"{1}\"%3E%3C/script%3E'))</script>";
    }
}