namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptHtmlTemplatesTransformer : IAssetTransformer
    {
        readonly string contentType;

        public WrapJavaScriptHtmlTemplatesTransformer(string contentType)
        {
            this.contentType = contentType;
        }

        public string Transform(string assetContent, IAsset asset)
        {
            return string.Format(
@"(function(d) {{
var addTemplate = function(id, content) {{
    var script = d.createElement('script');
    script.type = '{0}';
    script.id = id;
    if (typeof script.textContent !== 'undefined') {{
        script.textContent = content;
    }} else {{
        script.innerText = content;
    }}
    var x = d.getElementsByTagName('script')[0];
    x.parentNode.insertBefore(script, x);
}};
{1}
}}(document));",
                contentType,
                assetContent);
        }
    }
}