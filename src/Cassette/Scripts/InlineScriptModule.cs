using System;
using System.Web;

namespace Cassette.Scripts
{
    public class InlineScriptModule : ScriptModule
    {
        readonly string scriptContent;

        public InlineScriptModule(string scriptContent) : base("")
        {
            this.scriptContent = scriptContent;
        }

        public override IHtmlString Render()
        {
            return new HtmlString(
                "<script type=\"text/javascript\">" + Environment.NewLine + 
                scriptContent + Environment.NewLine + 
                "</script>"
                );
        }
    }
}