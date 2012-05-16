using System;

namespace Cassette.Scripts
{
#pragma warning disable 659
    class InlineScriptBundle : ScriptBundle
    {
        readonly string scriptContent;

        public InlineScriptBundle(string scriptContent)
        {
            this.scriptContent = scriptContent;
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
        }

        internal override string Render()
        {
            var content = string.Format(
                HtmlConstants.InlineScriptHtml,
                HtmlAttributes.CombinedAttributes,
                Environment.NewLine,
                scriptContent
            );

            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(Condition, html => html.Append(content));
        }

        public override bool Equals(object obj)
        {
            return Object.ReferenceEquals(obj, this);
        }
    }
#pragma warning restore 659
}