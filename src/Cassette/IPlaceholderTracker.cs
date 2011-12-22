using System;

namespace Cassette
{
    interface IPlaceholderTracker
    {
        string InsertPlaceholder(Func<string> futureHtml);
        string ReplacePlaceholders(string html);
    }
}