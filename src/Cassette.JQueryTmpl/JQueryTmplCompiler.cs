using Cassette.IO;
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

        public string Compile(string source, IFile sourceFile)
        {
            return CreateFunction(source);
        }

        protected virtual string CreateFunction(string source)
        {
            return ScriptEngine.CallGlobalFunction<string>("buildTmplFn", source);
        }
    }
}
