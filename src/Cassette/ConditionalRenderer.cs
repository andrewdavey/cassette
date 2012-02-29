using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.Scripts;

namespace Cassette
{
    public class ConditionalRenderer
    {
        public string RenderCondition(string condition, string content)
        {
            var html = new StringBuilder();
            
            // Check if the condition contains !IE.  Ignore any brackets or spaces
            bool notIECondition = condition.Replace(" ", "").Replace("(", "").Contains("!IE");

            // Append the start of the conditional, including a comment closer if !IE
            html.AppendFormat(HtmlConstants.ConditionalCommentStart, condition);
            if (notIECondition)
            {
                html.Append("-->");
            }
            html.AppendLine();

            // Append the content
            html.Append(content);

            // Append the end of the conditional, starting with a comment opener if !IE
            html.AppendLine();

            if (notIECondition)
            {
                html.Append("<!-- ");
            }

            html.Append(HtmlConstants.ConditionalCommentEnd);

            // Return the finished conditional
            return html.ToString();
        }
    }
}
