using System;
using System.Web;

namespace Cassette.Web
{
    public interface IPlaceholderTracker
    {
        IHtmlString InsertPlaceholder(string id, Func<IHtmlString> createHtml);
        string ReplacePlaceholders(string lineOfHtml);
    }
}
