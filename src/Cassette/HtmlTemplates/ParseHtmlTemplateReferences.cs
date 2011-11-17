using System.IO;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class ParseHtmlTemplateReferences : IBundleProcessor<HtmlTemplateBundle>
    {
        public void Process(HtmlTemplateBundle bundle, ICassetteApplication application)
        {
            foreach (var asset in bundle.Assets)
            {
                ProcessAsset(asset);
            }
        }

        void ProcessAsset(IAsset asset)
        {
            string code;
            using (var stream = asset.OpenStream())
            using (var reader = new StreamReader(stream))
            {
                code = reader.ReadToEnd();
            }

            var commentParser = new HtmlTemplateCommentParser();
            var referenceParser = new ReferenceParser(commentParser);
            var references = referenceParser.Parse(code, asset);
            foreach (var reference in references)
            {
                asset.AddReference(reference.Path, reference.LineNumber);
            }
        }
    }
}