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

            var hasCondition = !string.IsNullOrEmpty(Condition);
            if (hasCondition)
            {
                html.AppendFormat(HtmlConstants.ConditionalCommentStart, Condition);
                html.AppendLine();
            }

            html.Append(string.Format(
                HtmlConstants.InlineScriptHtml,
                HtmlAttributes.CombinedAttributes,
                Environment.NewLine,
                scriptContent
            ));

            if (hasCondition)
            {
                html.AppendLine();
                html.Append(HtmlConstants.ConditionalCommentEnd);
            }

            return html.ToString();
        }
    }
}