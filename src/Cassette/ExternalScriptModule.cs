using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Cassette
{
    public class ExternalScriptModule : ScriptModule
    {
        public ExternalScriptModule(string url)
            : base("", null)
        {
            this.url = url;
        }

        readonly string url;

        public override IHtmlString Render(ICassetteApplication application)
        {
            return base.Render(application);
        }
    }
}
