using System;
using System.Web;

namespace Knapsack.Web
{
    public interface IPageHelper
    {
        void ReferenceScript(string scriptPath);
        IHtmlString RenderScripts(string location);

        void ReferenceStylesheet(string stylesheetPath);
        IHtmlString RenderStylesheetLinks();

        string ReplacePlaceholders(string line);
    }
}
