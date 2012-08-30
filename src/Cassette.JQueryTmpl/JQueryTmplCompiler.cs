using System.Linq;
using Jurassic;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplCompiler : ICompiler
    {
        public JQueryTmplCompiler()
        {
            ScriptEngine = new ScriptEngine();
            ScriptEngine.Execute(Properties.Resources.jqueryTmplCompiler);
        }

        protected readonly ScriptEngine ScriptEngine;

        public CompileResult Compile(string source, CompileContext context)
        {
            var javascript = CreateFunction(source);
            return new CompileResult(javascript, Enumerable.Empty<string>());
        }

        protected virtual string CreateFunction(string source)
        {
            return ScriptEngine.CallGlobalFunction<string>("buildTmplFn", source);
        }
    }
}