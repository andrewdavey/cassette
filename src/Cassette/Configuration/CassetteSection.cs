using System.Configuration;
using System.Web;

namespace Cassette.Configuration
{
    public class CassetteSection : ConfigurationSection
    {
        [ConfigurationProperty("moduleMode", DefaultValue = ModuleMode.OffInDebug)]
        public ModuleMode ModuleMode
        {
            get { return (ModuleMode)this["moduleMode"]; }
            set { this["moduleMode"] = value; }
        }

        [ConfigurationCollection(typeof(ModuleElement))]
        [ConfigurationProperty("scripts")]
        public ModuleCollection Scripts
        {
            get { return (ModuleCollection)this["scripts"]; }
        }

        [ConfigurationCollection(typeof(ModuleElement))]
        [ConfigurationProperty("styles")]
        public ModuleCollection Styles
        {
            get { return (ModuleCollection)this["styles"]; }
        }

        [ConfigurationCollection(typeof(ModuleElement))]
        [ConfigurationProperty("htmlTemplates")]
        public ModuleCollection HtmlTemplates
        {
            get { return (ModuleCollection)this["htmlTemplates"]; }
        }

        [ConfigurationProperty("bufferHtmlOutput", DefaultValue=true)]
        public bool BufferHtmlOutput
        {
            get { return (bool)this["bufferHtmlOutput"]; }
            set { this["bufferHtmlOutput"] = value; }
        }

        [ConfigurationProperty("handler", DefaultValue="~/cassette.axd")]
        public string Handler
        {
            get { return (string)this["handler"]; }
            set { this["handler"] = value; }
        }

        public bool ShouldUseModules(HttpContextBase context)
        {
            return ModuleMode == ModuleMode.On
                || (ModuleMode == ModuleMode.OffInDebug 
                    && !context.IsDebuggingEnabled);
        }
    }
}
