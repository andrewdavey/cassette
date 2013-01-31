﻿using System.Collections.Generic;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsSettings
    {
        /// <summary>
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </summary>
        public string JavaScriptVariableName { get; set; }

        /// <summary>
        /// A list of any custom Handlebars helpers that are needed during precompilation.
        /// For example, if you have templates that make use of a "t" helper with <code>{{t "MyKey"}}</code>
        /// then you would add "t" to the list.
        /// </summary>
        public List<string> KnownHelpers { get; set; }

        /// <summary>
        /// The flag which determines if you only wish to precompile your Handlebars 
        /// templates with ONLY your Known Helpers.
        /// </summary>
        public bool KnownHelpersOnly { get; set; }

        public HandlebarsSettings()
        {
            JavaScriptVariableName = "JST";
            KnownHelpers = new List<string>();
            KnownHelpersOnly = false;
        }

        public HandlebarsSettings(IEnumerable<IConfiguration<HandlebarsSettings>> configurations)
        {
            JavaScriptVariableName = "JST";
            KnownHelpers = new List<string>();
            KnownHelpersOnly = false;
            ApplyConfigurations(configurations);
        }

        void ApplyConfigurations(IEnumerable<IConfiguration<HandlebarsSettings>> configurations)
        {
            configurations.OrderByConfigurationOrderAttribute().Configure(this);
        }
    }
}