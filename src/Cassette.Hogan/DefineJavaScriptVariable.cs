namespace Cassette.HtmlTemplates
{
    class DefineJavaScriptVariable : IAssetTransformer
    {
        readonly string javaScriptVariableName;

        public DefineJavaScriptVariable(string javaScriptVariableName)
        {
            this.javaScriptVariableName = javaScriptVariableName;
        }

        public string Transform(string source, IAsset asset)
        {
            return string.Format(
                "if (typeof {0}==='undefined'){{var {0}={{}};}}{1}",
                javaScriptVariableName,
                source
                );
        }
    }
}