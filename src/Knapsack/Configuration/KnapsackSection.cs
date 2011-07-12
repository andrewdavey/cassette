using System.Configuration;
using System.Web;

namespace Knapsack.Configuration
{
    public class KnapsackSection : ConfigurationSection
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

        [ConfigurationProperty("bufferHtmlOutput", DefaultValue=true)]
        public bool BufferHtmlOutput
        {
            get { return (bool)this["bufferHtmlOutput"]; }
            set { this["bufferHtmlOutput"] = value; }
        }

        [ConfigurationProperty("handler", DefaultValue="~/knapsack.axd")]
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
