using System;
using Cassette.Configuration;
using System.Text;

namespace Cassette.Scripts
{
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
            var html = new StringBuilder();

            html.Append(string.Format(
                HtmlConstants.InlineScriptHtml,
                HtmlAttributes.CombinedAttributes,
                Environment.NewLine,
                scriptContent
            ));

            if (HasCondition)
            {
                return new ConditionalRenderer().RenderCondition(Condition, html.ToString());
            }
            else
            {
                return html.ToString();
            }
        }
    }
}