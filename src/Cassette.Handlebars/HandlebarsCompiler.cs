using System.Linq;
using Jurassic;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsCompiler : ICompiler
    {
        public HandlebarsCompiler()
        {
            _scriptEngine = new ScriptEngine();
            _scriptEngine.Execute(Properties.Resources.handlebars);
            _scriptEngine.Execute("handlebarsPrecompile = Handlebars.precompile;");
        }

        readonly ScriptEngine _scriptEngine;

        public CompileResult Compile(string source, CompileContext context)
        {
            var javascript = _scriptEngine.CallGlobalFunction<string>("handlebarsPrecompile", source);
            return new CompileResult(javascript, Enumerable.Empty<string>());
        }
    }
}