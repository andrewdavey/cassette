using System;
using System.Web;

namespace Cassette
{
    class NullPlaceholderTracker : IPlaceholderTracker
    {
        public IHtmlString InsertPlaceholder(Func<IHtmlString> futureHtml)
        {
            return futureHtml();
        }

        public string ReplacePlaceholders(string html)
        {
            return html;
        }
    }
}