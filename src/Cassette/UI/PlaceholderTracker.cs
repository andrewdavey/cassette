using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Cassette.UI
{
    public class PlaceholderTracker
    {
        readonly Dictionary<string, Func<IHtmlString>> creationFunctions = new Dictionary<string, Func<IHtmlString>>();

        public IHtmlString InsertPlaceholder(string id, Func<IHtmlString> createHtml)
        {
            creationFunctions[id] = createHtml;
            return new HtmlString(Environment.NewLine + id + Environment.NewLine);
        }

        public string ReplacePlaceholders(string html)
        {
            var builder = new StringBuilder(html);
            foreach (var item in creationFunctions)
            {
                builder.Replace(item.Key, item.Value().ToHtmlString());
            }
            return builder.ToString();
        }
    }
}
