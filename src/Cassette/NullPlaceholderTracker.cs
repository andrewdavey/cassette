using System;

namespace Cassette
{
    /// <summary>
    /// A do-nothing implementation of <see cref="IPlaceholderTracker"/>.
    /// </summary>
    class NullPlaceholderTracker : IPlaceholderTracker
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
