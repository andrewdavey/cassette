using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Cassette.UI
{
    public class PlaceholderTracker : IPlaceholderTracker
    {
        readonly Dictionary<Guid, string> creationFunctions = new Dictionary<Guid, string>();

        public IHtmlString InsertPlaceholder(IHtmlString futureHtml)
        {
            var id = Guid.NewGuid();
            creationFunctions[id] = futureHtml.ToHtmlString();
            return new HtmlString(Environment.NewLine + id.ToString() + Environment.NewLine);
        }

        public string ReplacePlaceholders(string html)
        {
            var builder = new StringBuilder(html);
            foreach (var item in creationFunctions)
            {
                builder.Replace(item.Key.ToString(), item.Value);
            }
            return builder.ToString();
        }
    }
}
