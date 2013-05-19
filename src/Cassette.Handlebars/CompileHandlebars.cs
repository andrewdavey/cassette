using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileHandlebars : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly CassetteSettings settings;
        readonly HandlebarsSettings handlebarsSettings;

        public CompileHandlebars(CassetteSettings settings)
        {
            this.settings = settings;
        }

        public CompileHandlebars(CassetteSettings settings, HandlebarsSettings handlebarsSettings)
        {
            this.settings = settings;
            this.handlebarsSettings = handlebarsSettings;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new CompileAsset(new HandlebarsCompiler(handlebarsSettings), settings.SourceDirectory);
        }
    }
}