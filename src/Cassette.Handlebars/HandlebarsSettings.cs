using System.Collections.Generic;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsSettings
    {
        /// <summary>
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </summary>
        public string JavaScriptVariableName { get; set; }

        public HandlebarsSettings(IEnumerable<IConfiguration<HandlebarsSettings>> configurations)
        {
            JavaScriptVariableName = "JST";
            ApplyConfigurations(configurations);
        }

        void ApplyConfigurations(IEnumerable<IConfiguration<HandlebarsSettings>> configurations)
        {
            configurations.OrderByConfigurationOrderAttribute().Configure(this);
        }
    }
}