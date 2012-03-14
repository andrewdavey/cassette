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
            return Regex.Replace(
                html,
                "<CASSETTE_URL_ROOT>(.*?)</CASSETTE_URL_ROOT>",
                match => urlModifier.Modify(match.Groups[1].Value)
            );
        }
    }
}