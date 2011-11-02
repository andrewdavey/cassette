using System;

namespace Cassette.UI
{
    public class NullPlaceholderTracker : IPlaceholderTracker
    {
        public string InsertPlaceholder(Func<string> futureHtml)
        {
            return futureHtml();
        }

        public string ReplacePlaceholders(string html)
        {
            return html;
        }
    }
}