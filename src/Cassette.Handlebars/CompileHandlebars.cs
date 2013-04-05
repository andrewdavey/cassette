using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates {
    public class CompileHandlebars : AddTransformerToAssets<HtmlTemplateBundle> {
        readonly CassetteSettings settings;

        public CompileHandlebars(CassetteSettings settings) {
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle) {
            return new CompileAsset(new HandlebarsCompiler(), settings.SourceDirectory);
        }
    }
}