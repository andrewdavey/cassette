using System;
using System.Web;

namespace Cassette.UI
{
    public interface IPlaceholderTracker
    {
        IHtmlString InsertPlaceholder(IHtmlString futureHtml);
        string ReplacePlaceholders(string html);
    }
}
