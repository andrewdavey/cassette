using System.Configuration;

namespace Cassette.Configuration
{
    public class ModuleElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsKey = true, IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("location", DefaultValue = null)]
        public string Location
        {
            get { return (string)this["location"]; }
            set { this["location"] = value; }
        }
    }
}
