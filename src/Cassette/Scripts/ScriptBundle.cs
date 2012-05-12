using System.Xml.Linq;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ScriptBundle : Bundle
    {
        public ScriptBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/javascript";
            HtmlAttributes["type"] = "text/javascript";
        }

        protected ScriptBundle()
        {
            HtmlAttributes["type"] = "text/javascript";
        }

        /// <summary>
        /// The Internet Explorer specific condition used control if the script should be loaded using an HTML conditional comment.
        /// For example, <example>"lt IE 9"</example>.
        /// </summary>
        public string Condition { get; set; }

        public IBundlePipeline<ScriptBundle> Pipeline { get; set; }

        public IBundleHtmlRenderer<ScriptBundle> Renderer { get; set; }

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
            var serializer = new ScriptBundleSerializer(container);
            serializer.Serialize(this);
        }

        protected override string UrlBundleTypeArgument
        {
            get { return "script"; }
        }
    }
}