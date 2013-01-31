using System.Linq;
using Jurassic;
using Newtonsoft.Json;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsCompiler : ICompiler
    {
        public HandlebarsCompiler(HandlebarsSettings settings)
        {
            scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.handlebars);

            var knownHelpersJson = JsonConvert.SerializeObject(settings.KnownHelpers);
            var knownHelpersOnly = JsonConvert.False;
            if (settings.KnownHelpersOnly) {
                knownHelpersOnly = JsonConvert.True;
            }

            // Create a global compile function that returns the template compiled into javascript source.
            scriptEngine.Execute(@"
                var precompile = function (template) {
                    return Handlebars.precompile(template, { knownHelpers: " + knownHelpersJson + @", knownHelpersOnly: " + knownHelpersOnly + @" });
                };"
            );
        }
        
        readonly ScriptEngine scriptEngine;

        public CompileResult Compile(string source, CompileContext context)
        {
            var javascript = scriptEngine.CallGlobalFunction<string>("precompile", source);
            //var handlebarsJs = string.Format("var {0} = Handlebars.template({1});", source, scriptEngine.CallGlobalFunction("precompile", source));
            return new CompileResult(javascript, Enumerable.Empty<string>());
        }
    }
}