using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Cassette.UI
{
    public class PlaceholderTracker : IPlaceholderTracker
    {
        readonly Dictionary<Guid, Func<IHtmlString>> creationFunctions = new Dictionary<Guid, Func<IHtmlString>>();

        public IHtmlString InsertPlaceholder(Func<IHtmlString> futureHtml)
        {
            var id = Guid.NewGuid();
            creationFunctions[id] = futureHtml;
            return new HtmlString(Environment.NewLine + id.ToString() + Environment.NewLine);
        }

        public string ReplacePlaceholders(string html)
        {
            var builder = new StringBuilder(html);
            foreach (var item in creationFunctions)
            {
                builder.Replace(item.Key.ToString(), item.Value().ToHtmlString());
            }
            return builder.ToString();
        }
    }
}
