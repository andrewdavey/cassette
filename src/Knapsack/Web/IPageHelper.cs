using System;
using System.Web;

namespace Knapsack.Web
{
    public interface IPageHelper
    {
        void ReferenceScript(string scriptPath);
        void ReferenceStylesheet(string stylesheetPath);
        IHtmlString RenderScripts();
        IHtmlString RenderStylesheetLinks();
    }
}
