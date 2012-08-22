using System.Text.RegularExpressions;

namespace Cassette
{
    class ConstantHtmlRenderer<T> : IBundleHtmlRenderer<T>
        where T : Bundle
    {
        readonly string html;
        readonly IUrlModifier urlModifier;
        private readonly IApplicationRootPrepender applicationRootPrepender;

        public ConstantHtmlRenderer(string html, IUrlModifier urlModifier, IApplicationRootPrepender applicationRootPrepender)
        {
            this.html = html;
            this.urlModifier = urlModifier;
            this.applicationRootPrepender = applicationRootPrepender;
        }

        public string Render(T bundle)
        {
            var output = Regex.Replace(
                html,
                "<CASSETTE_URL_ROOT>(.*?)</CASSETTE_URL_ROOT>",
                match => urlModifier.Modify(match.Groups[1].Value)
            );

            return Regex.Replace(
                output,
                "<CASSETTE_APPLICATION_ROOT>(.*?)</CASSETTE_APPLICATION_ROOT>",
                match => applicationRootPrepender.Modify(match.Groups[1].Value)
            );
        }
    }
}