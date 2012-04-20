using System.Collections.Generic;

namespace Cassette.HtmlTemplates
{
    public class HoganSettings
    {
        /// <summary>
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </summary>
        public string JavaScriptVariableName { get; set; }

        public HoganSettings(IEnumerable<IConfiguration<HoganSettings>> configurations)
        {
            JavaScriptVariableName = "JST";
            ApplyConfigurations(configurations);
        }

        void ApplyConfigurations(IEnumerable<IConfiguration<HoganSettings>> configurations)
        {
            foreach (var configuration in configurations)
            {
                configuration.Configure(this);
            }
        }
    }
}