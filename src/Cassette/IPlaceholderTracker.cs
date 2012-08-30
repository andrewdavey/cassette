using System;

namespace Cassette
{
    public interface IPlaceholderTracker
    {
        string InsertPlaceholder(Func<string> futureHtml);
        string ReplacePlaceholders(string html);
    }
}