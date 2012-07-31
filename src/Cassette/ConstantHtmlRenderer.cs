using System.Text.RegularExpressions;

namespace Cassette
{
    class ConstantHtmlRenderer<T> : IBundleHtmlRenderer<T>
        where T : Bundle
    {
        readonly string html;
        readonly IUrlModifier urlModifier;

        public ConstantHtmlRenderer(string html, IUrlModifier urlModifier)
        {
            this.html = html;
            this.urlModifier = urlModifier;
        }

        public string Render(T bundle)
        {
            return Regex.Replace(html, "([^\"]+_cassette[^\"]+)\"", match => urlModifier.Modify(match.Captures[0].Value));
        }
    }
}