using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Cassette.Web
{
    public class HtmlTemplateAssetManager
    {
        public AssetLocator Reference(params string[] paths)
        {
            return new AssetLocator();
        }

        public IHtmlString Render()
        {
            return null;
        }
    }
}
