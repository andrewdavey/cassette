using System;
using System.Web;

namespace Knapsack.Web
{
    public interface IPageHelper
    {
        void AddScriptReference(string scriptPath);
        void AddStylesheet(string cssPath);
        IHtmlString RenderScripts();
        IHtmlString RenderStyleLinks();
    }
}
