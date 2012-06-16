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
            
            // Create a global compile function that returns the template compiled into javascript source.
            scriptEngine.Execute(@"
var compile = function (template) {
  return Hogan.compile(template, { asString: true });
};");
        }

        readonly ScriptEngine scriptEngine;

        public CompileResult Compile(string source, CompileContext context)
        {
            var javascript = scriptEngine.CallGlobalFunction<string>("compile", source);
            return new CompileResult(javascript, Enumerable.Empty<string>());
        }
    }
}