using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ScriptBundle : Bundle
    {
        public ScriptBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/javascript";
        }

        protected ScriptBundle()
        {
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

        protected override string UrlBundleTypeArgument
        {
            get { return "script"; }
        }
    }
}