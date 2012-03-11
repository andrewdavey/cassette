using System.Text;
using Cassette.Scripts;

namespace Cassette
{
    /// <summary>
    /// Renders conditional comments as described by http://www.quirksmode.org/css/condcom.html
    /// </summary>
    class ConditionalRenderer
    {
        StringBuilder html;
        bool isNotIECondition;

        public string RenderCondition(string condition, string content)
        {
            html = new StringBuilder();            
            isNotIECondition = ConditionContainsNotIE(condition);

            StartComment(condition);
            ContentWrappedInLineBreaks(content);
            EndComment();

            return html.ToString();
        }

        void StartComment(string condition)
        {
            html.AppendFormat(HtmlConstants.ConditionalCommentStart, condition);
            if (isNotIECondition)
            {
                html.Append("<!-->");
            }
        }

        void ContentWrappedInLineBreaks(string content)
        {
            html.AppendLine();
            html.Append(content);
            html.AppendLine();
        }

        void EndComment()
        {
            if (isNotIECondition)
            {
                html.Append("<!-- ");
            }
            html.Append(HtmlConstants.ConditionalCommentEnd);
        }

        /// <summary>
        /// Check if the condition contains '!IE". Ignores any brackets or spaces.
        /// </summary>
        bool ConditionContainsNotIE(string condition)
        {
            return condition
                .Replace(" ", "")
                .Replace("(", "")
                .Contains("!IE");
        }
    }
}