using System;

namespace Cassette.HtmlTemplates
{
    public class GlobalVarHtmlTemplateScriptStrategy : IHtmlTemplateScriptStrategy
    {
        readonly string globalVarName;
        readonly IJsonSerializer jsonSerializer;

        public GlobalVarHtmlTemplateScriptStrategy(string globalVarName, IJsonSerializer jsonSerializer)
        {
            if (globalVarName == null) throw new ArgumentNullException("globalVarName");
            if (jsonSerializer == null) throw new ArgumentNullException("jsonSerializer");
            this.globalVarName = globalVarName;
            this.jsonSerializer = jsonSerializer;
        }

        public string DefineTemplate(string templateId, string templateContent)
        {
            return string.Format(
                "t[{0}]={1};",
                JavaScriptString(templateId),
                JavaScriptString(templateContent)
            );
        }

        string JavaScriptString(string str)
        {
            return jsonSerializer.Serialize(str);
        }

        public string WrapTemplates(string templateDefinitionScripts)
        {
            return string.Format(
@"(function(t){{
{1}
}}(window.{0}||(window.{0}={{}})));", globalVarName, templateDefinitionScripts);
        }
    }
}