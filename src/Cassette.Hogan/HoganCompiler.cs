using Cassette.IO;
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

        public string Compile(string source, IFile sourceFile)
        {
            return scriptEngine.CallGlobalFunction<string>("compile", source);
        }
    }
}