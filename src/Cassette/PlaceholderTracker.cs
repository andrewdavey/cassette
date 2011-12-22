using System;
using System.Collections.Generic;
using System.Text;

namespace Cassette
{
    class PlaceholderTracker : IPlaceholderTracker
    {
        readonly Dictionary<Guid, Func<string>> creationFunctions = new Dictionary<Guid, Func<string>>();

        public string InsertPlaceholder(Func<string> futureHtml)
        {
            var id = Guid.NewGuid();
            creationFunctions[id] = futureHtml;
            return Environment.NewLine + id + Environment.NewLine;
        }

        public string ReplacePlaceholders(string html)
        {
            var builder = new StringBuilder(html);
            foreach (var item in creationFunctions)
            {
                builder.Replace(item.Key.ToString(), item.Value());
            }
            return builder.ToString();
        }
    }
}