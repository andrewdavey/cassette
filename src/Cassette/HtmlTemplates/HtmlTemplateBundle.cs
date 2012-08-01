using System;
using System.Xml.Linq;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle : Bundle
    {
        public HtmlTemplateBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/html";
        }

        public IBundlePipeline<HtmlTemplateBundle> Pipeline { get; set; }
        
        public IBundleHtmlRenderer<HtmlTemplateBundle> Renderer { get; set; }

        protected override void ProcessCore(CassetteSettings settings)
        {
            Pipeline.Process(this);
        }

        internal override string Render()
        {
            return Renderer.Render(this);
        }

        internal override void SerializeInto(XContainer container)
        {
            var serializer = new HtmlTemplateBundleSerializer(container);
            serializer.Serialize(this);
        }

        internal string GetTemplateId(IAsset asset)
        {
            string id;
            if (asset.Path.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
            {
                id = asset.Path
                    .Substring(Path.Length + 1)
                    .Replace(System.IO.Path.DirectorySeparatorChar, '-')
                    .Replace(System.IO.Path.AltDirectorySeparatorChar, '-');
            }
            else
            {
                id = asset.Path
                    .Substring(2) // Skips the "~/" prefix
                    .Replace(System.IO.Path.DirectorySeparatorChar, '-')
                    .Replace(System.IO.Path.AltDirectorySeparatorChar, '-');
            }
            return System.IO.Path.GetFileNameWithoutExtension(id);
        }

        protected override string UrlBundleTypeArgument
        {
            get { return "htmltemplate"; }
        }
    }
}