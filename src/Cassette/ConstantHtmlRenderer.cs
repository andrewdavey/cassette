namespace Cassette
{
    class ConstantHtmlRenderer<T> : IBundleHtmlRenderer<T>
        where T : Bundle
    {
        readonly string html;

        public ConstantHtmlRenderer(string html)
        {
            this.html = html;
        }

        public string Render(T bundle)
        {
            return html;
        }
    }
}