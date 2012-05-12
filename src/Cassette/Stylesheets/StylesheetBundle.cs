using System.Xml.Linq;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetBundle : Bundle
    {
        public StylesheetBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/css";
            HtmlAttributes["type"] = "text/css";
            HtmlAttributes["rel"] = "stylesheet";
        }

        /// <summary>
        /// The value of the media attribute for this stylesheet's link element. For example, <example>print</example>.
        /// </summary>
        public string Media
        {
            get
            {
                string value;
                return HtmlAttributes.TryGetValue("media", out value) ? value : null;
            }
            set { HtmlAttributes["media"] = value; }
        }

        /// <summary>
        /// The Internet Explorer specific condition used control if the stylesheet should be loaded using an HTML conditional comment.
        /// For example, <example>"lt IE 9"</example>.
        /// </summary>
        public string Condition { get; set; }

        public IBundlePipeline<StylesheetBundle> Pipeline { get; set; }

        public IBundleHtmlRenderer<StylesheetBundle> Renderer { get; set; }

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
            var serializer = new StylesheetBundleSerializer(container);
            serializer.Serialize(this);
        }

        protected override string UrlBundleTypeArgument
        {
            get { return "stylesheet"; }
        }
    }
}