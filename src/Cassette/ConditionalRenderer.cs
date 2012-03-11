using System;
using System.Text;

namespace Cassette
{
    /// <summary>
    /// Renders conditional comments as described by http://www.quirksmode.org/css/condcom.html,
    /// except for a slight change in the "!IE" case for IE9 compatibility.
    /// </summary>
    class ConditionalRenderer
    {
        StringBuilder html;
// ReSharper disable InconsistentNaming
        bool isNotIECondition;
// ReSharper restore InconsistentNaming

        const string ConditionalCommentStart = "<!--[if {0}]>";
        const string ConditionalCommentEnd = "<![endif]-->";

        public string Render(string condition, Action<StringBuilder> appendContent)
        {
            html = new StringBuilder();
            if (string.IsNullOrEmpty(condition))
            {
                appendContent(html);
            }
            else
            {
                WrapConditionalCommentAroundContent(condition, appendContent);
            }
            return html.ToString();
        }

        void WrapConditionalCommentAroundContent(string condition, Action<StringBuilder> appendContent)
        {
            isNotIECondition = ConditionContainsNotIE(condition);
            StartComment(condition);
            ContentWrappedInLineBreaks(appendContent);
            EndComment();
        }

        void StartComment(string condition)
        {
            html.AppendFormat(ConditionalCommentStart, condition);
            if (isNotIECondition)
            {
                html.Append("<!-->");
            }
        }

        void ContentWrappedInLineBreaks(Action<StringBuilder> appendContent)
        {
            html.AppendLine();
            appendContent(html);
            html.AppendLine();
        }

        void EndComment()
        {
            if (isNotIECondition)
            {
                html.Append("<!-- ");
            }
            html.Append(ConditionalCommentEnd);
        }

        /// <summary>
        /// Check if the condition contains '!IE". Ignores any brackets or spaces.
        /// </summary>
// ReSharper disable InconsistentNaming
        static bool ConditionContainsNotIE(string condition)
// ReSharper restore InconsistentNaming
        {
            return condition
                .Replace(" ", "")
                .Replace("(", "")
                .Contains("!IE");
        }
    }
}