using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Cassette
{
    public class PlaceholderTracker : IPlaceholderTracker
    {
        readonly Dictionary<string, Func<IHtmlString>> creationFunctions = new Dictionary<string, Func<IHtmlString>>();

        public IHtmlString InsertPlaceholder(string id, Func<IHtmlString> createHtml)
        {
            creationFunctions[id] = createHtml;
            return new HtmlString(Environment.NewLine + id + Environment.NewLine);
        }

        public string ReplacePlaceholders(string lineOfHtml)
        {
            foreach (var item in creationFunctions)
            {
                if (lineOfHtml.StartsWith(item.Key))
                {
                    return item.Value().ToHtmlString();
                }
            }

            // No replacements.
            return lineOfHtml;
        }
    }
}
