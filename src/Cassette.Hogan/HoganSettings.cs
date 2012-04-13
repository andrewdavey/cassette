namespace Cassette.HtmlTemplates
{
    public class HoganSettings
    {
        /// <summary>
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </summary>
        public string JavaScriptVariableName { get; set; }

        public HoganSettings()
        {
            JavaScriptVariableName = "JST";
        }
    }
}