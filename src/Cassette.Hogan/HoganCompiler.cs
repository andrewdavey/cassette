using System.Linq;
using Jurassic;

namespace Cassette.HtmlTemplates
{
    public class HoganCompiler : ICompiler
    {
        public HoganCompiler()
        {
            scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.hogan);
        }

        readonly ScriptEngine scriptEngine;

        public CompileResult Compile(string source, CompileContext context)
        {
            var javascript = scriptEngine.CallGlobalFunction<string>("compile", source);
            return new CompileResult(javascript, Enumerable.Empty<string>());
        }
    }
}