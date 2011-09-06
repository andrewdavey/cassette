namespace Cassette.HtmlTemplates
{
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

        string Rewrite(string source)
        {
            return ScriptEngine.CallGlobalFunction<string>("rewriteTemplate", source);
        }
    }
}