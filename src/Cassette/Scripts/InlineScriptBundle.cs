using System;
using System.Text.RegularExpressions;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    internal class InlineScriptBundle : ScriptBundle
    {
        readonly string scriptContent;

        public InlineScriptBundle(string scriptContent)
        {
            this.scriptContent = scriptContent;
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
        }

        static readonly Regex DetectScriptRegex = new Regex(@"\A \s* <script \b",
                                                            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        /// <summary>
        /// Handle cases of the content already wrapped in a &lt;script&gt; tag.
        /// </summary>
        /// <returns></returns>
        protected string GetScriptContent()
        {
            var isContentScriptTag = scriptContent != null &&
                                     DetectScriptRegex.IsMatch(scriptContent, 0);
            var htmlAttributes = HtmlAttributes.CombinedAttributes; // should start with a space

            if (isContentScriptTag)
            {
                return DetectScriptRegex.Replace(scriptContent,
                    m => m.Value + htmlAttributes, 1, 0); // don't need a space after the attributes - the regex is checking for "\b"
            }
            return String.Format(
                HtmlConstants.InlineScriptHtml,
                htmlAttributes,
                Environment.NewLine,
                scriptContent
                );
        }

        internal override string Render()
        {
            var content = GetScriptContent();
            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(Condition, html => html.Append(content));
        }
    }
}