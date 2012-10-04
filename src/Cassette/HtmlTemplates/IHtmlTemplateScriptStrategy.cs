namespace Cassette.HtmlTemplates
{
    /// <summary>
    /// Used to convert HTML templates into JavaScript.
    /// </summary>
    public interface IHtmlTemplateScriptStrategy
    {
        /// <summary>
        /// Returns a JavaScript that will define an HTML template with the given id and content.
        /// </summary>
        /// <param name="templateId">The ID of the HTML template.</param>
        /// <param name="templateContent">The content of the HTML template.</param>
        string DefineTemplate(string templateId, string templateContent);

        /// <summary>
        /// Wraps HTML templates definition JavaScript with additional JavaScript that
        /// may set up additional context e.g. define helper functions or variables.
        /// </summary>
        /// <param name="templateDefinitionScripts"></param>
        string WrapTemplates(string templateDefinitionScripts);
    }
}