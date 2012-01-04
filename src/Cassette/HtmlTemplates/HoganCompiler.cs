using System.Text;
using Cassette.IO;
using Jurassic;
using Jurassic.Library;

namespace Cassette.HtmlTemplates
{
    public class HoganCompiler : ICompiler
    {
        public HoganCompiler()
        {
            ScriptEngine = new ScriptEngine();
            ScriptEngine.Execute(Properties.Resources.hogan);
        }

        protected readonly ScriptEngine ScriptEngine;

        public string Compile(string source, IFile sourceFile)
        {
            return CreateFunction(source);
        }

        protected virtual string CreateFunction(string source)
        {
            return ScriptEngine.CallGlobalFunction<string>("compile", source);
        }
    }
}
