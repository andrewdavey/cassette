using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHandlebars : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly IHtmlTemplateIdStrategy idStrategy;

        public delegate RegisterTemplatesWithHandlebars Factory(string javaScriptVariableName);

        readonly string javaScriptVariableName;

        public RegisterTemplatesWithHandlebars(string javaScriptVariableName, IHtmlTemplateIdStrategy idStrategy)
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
            return new RegisterTemplateWithHandlebars(bundle, javaScriptVariableName, idStrategy);
        }
    }
}