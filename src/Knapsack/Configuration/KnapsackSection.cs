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

        public bool ShouldUseModules(HttpContextBase context)
        {
            return ModuleMode == ModuleMode.On
                || (ModuleMode == ModuleMode.OffInDebug 
                    && !context.IsDebuggingEnabled);
        }
    }
}
