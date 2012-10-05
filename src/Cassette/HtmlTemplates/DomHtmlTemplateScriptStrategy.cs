namespace Cassette.HtmlTemplates
{
    public class DomHtmlTemplateScriptStrategy : IHtmlTemplateScriptStrategy
    {
        readonly IJsonSerializer jsonSerializer;

        public DomHtmlTemplateScriptStrategy(IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
        }

        public string DefineTemplate(string templateId, string templateContent)
        {
            return string.Format("addTemplate({0},{1});", JavaScriptString(templateId), JavaScriptString(templateContent));
        }

        string JavaScriptString(string str)
        {
            return jsonSerializer.Serialize(str);
        }

        public string WrapTemplates(string templateDefinitionScripts)
        {
            // TODO: parameterize script type so we can pass in Bundle.ContentType?

            return @"(function(d){
var addTemplate = function(id, content) {{
    var script = d.createElement('script');
    script.type = 'text/html';
    script.id = id;
    if (typeof script.textContent !== 'undefined') {{
        script.textContent = content;
    }} else {{
        script.innerText = content;
    }}
    var x = d.getElementsByTagName('script')[0];
    x.parentNode.insertBefore(script, x);
}};
" + templateDefinitionScripts + @"
}(document));";
        }
    }
}