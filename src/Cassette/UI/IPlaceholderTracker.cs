using System;
using System.Web;

namespace Cassette.UI
{
    public interface IPlaceholderTracker
    {
        IHtmlString InsertPlaceholder(Func<IHtmlString> futureHtml);
        string ReplacePlaceholders(string html);
    }
}
