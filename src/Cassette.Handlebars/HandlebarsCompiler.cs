using System.Linq;
using Jurassic;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsCompiler : ICompiler
    {
        public HandlebarsCompiler(HandlebarsSettings settings)
        {
            scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.handlebars);

            var knownHelpersFormattedList = settings.KnownHelpers.Aggregate("[", (current, helper) => current + ("'" + helper + "'"));
            knownHelpersFormattedList += "]";


            // Create a global compile function that returns the template compiled into javascript source.
            scriptEngine.Execute(@"
                var precompile = function (template) {
                    return Handlebars.precompile(template, { knownHelpers: "+ knownHelpersFormattedList +@", knownHelpersOnly: false });
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