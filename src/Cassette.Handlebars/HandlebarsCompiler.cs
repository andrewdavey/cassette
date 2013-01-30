using System.Linq;
using Jurassic;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsCompiler : ICompiler
    {
        public HandlebarsCompiler()
        {
            scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.handlebars);
            
            scriptEngine.Execute(@"var precompile = Handlebars.precompile;");
        }


        readonly ScriptEngine scriptEngine;

        public CompileResult Compile(string source, CompileContext context)
        {
            //var javascript = scriptEngine.CallGlobalFunction<string>("precompile", source);
            var handlebarsJs = string.Format("var {0} = Handlebars.template({1});", source, scriptEngine.CallGlobalFunction("precompile", source));
            return new CompileResult(handlebarsJs, Enumerable.Empty<string>());
        }
    }
}