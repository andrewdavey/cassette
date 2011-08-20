using System.IO;
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

        public string Compile(string source, string filename, IFileSystem currentDirectory)
        {
            var function = CreateFunction(source);
            return string.Format(
                "$.template(\"{0}\", {1});",
                Path.GetFileNameWithoutExtension(filename),
                function
            );
        }

        protected virtual string CreateFunction(string source)
        {
            return ScriptEngine.CallGlobalFunction<string>("buildTmplFn", source);
        }
    }

    public class KnockoutJQueryTmplCompiler : JQueryTmplCompiler
    {
        public KnockoutJQueryTmplCompiler()
        {
            ScriptEngine.Execute(Properties.Resources.jqueryTmplKnockout);
        }

        protected override string CreateFunction(string source)
        {
            return base.CreateFunction(Rewrite(source));
        }

        private string Rewrite(string source)
        {
            return ScriptEngine.CallGlobalFunction<string>("rewriteTemplate", source);
        }
    }
}
