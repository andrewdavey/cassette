namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithHogan : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly string javaScriptVariableName;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public RegisterTemplateWithHogan(HtmlTemplateBundle bundle, string javaScriptVariableName, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.javaScriptVariableName = javaScriptVariableName;
            this.idStrategy = idStrategy;
        }

        public string Transform(string compiledTemplate, IAsset asset)
        {
            var id = idStrategy.HtmlTemplateId(bundle, asset);
            var template = javaScriptVariableName + "['" + id + "']";

            var output = template + "=new Hogan.Template();" +
                         template + ".r=" + compiledTemplate + ";";
            return output;
        }
    }
}