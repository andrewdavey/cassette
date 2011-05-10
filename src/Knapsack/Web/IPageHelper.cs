using System;
using System.Web;

namespace Knapsack.Web
{
    public interface IPageHelper
    {
        void AddScriptReference(string scriptPath);
        IHtmlString RenderScripts();
    }
}
