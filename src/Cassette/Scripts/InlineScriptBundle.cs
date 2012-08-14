using System;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    [ProtoBuf.ProtoContract]
    class InlineScriptBundle : ScriptBundle
    {
        [ProtoBuf.ProtoMember(1)]
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
    }
}