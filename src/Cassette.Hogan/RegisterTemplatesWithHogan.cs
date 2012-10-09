using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHogan : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly IHtmlTemplateIdStrategy idStrategy;

        public delegate RegisterTemplatesWithHogan Factory(string javaScriptVariableName);

        readonly string javaScriptVariableName;

        public RegisterTemplatesWithHogan(string javaScriptVariableName, IHtmlTemplateIdStrategy idStrategy)
        {
            this.idStrategy = idStrategy;
            this.javaScriptVariableName = javaScriptVariableName ?? "JST";
        }

        public override void Process(HtmlTemplateBundle bundle)
        {
            base.Process(bundle);
            DefineJavaScriptVariableInFirstAsset(bundle);
        }

        void DefineJavaScriptVariableInFirstAsset(HtmlTemplateBundle bundle)
        {
            if (bundle.Assets.Count > 0)
            {
                bundle.Assets[0].AddAssetTransformer(new DefineJavaScriptVariable(javaScriptVariableName));
            }
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new RegisterTemplateWithHogan(bundle, javaScriptVariableName, idStrategy);
        }
    }
}