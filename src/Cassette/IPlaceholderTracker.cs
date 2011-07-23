using System;
using System.Web;

namespace Cassette
{
    public interface IPlaceholderTracker
    {
        IHtmlString InsertPlaceholder(string id, Func<IHtmlString> createHtml);
        string ReplacePlaceholders(string lineOfHtml);
    }
}
