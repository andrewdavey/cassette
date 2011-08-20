using System.IO;
using Jurassic;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplCompiler : ICompiler
    {
        public JQueryTmplCompiler()
        {
            scriptEngine = new ScriptEngine();
            scriptEngine.Execute(Properties.Resources.jqueryTmplCompiler);
        }

        readonly ScriptEngine scriptEngine;

        public string Compile(string source, string filename, IFileSystem currentDirectory)
        {
            var function = scriptEngine.CallGlobalFunction<string>("buildTmplFn", source);
            return string.Format(
                "$.template(\"{0}\", {1});",
                Path.GetFileNameWithoutExtension(filename),
                function
            );
        }
    }
}
