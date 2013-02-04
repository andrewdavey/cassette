using Jurassic;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsCompiler_Tests
    {
        [Fact]
        public void CanCompileHandlebarsTemplate()
        {
            var compiler = new HandlebarsCompiler();
            var result = compiler.Compile("Hello {{world}}", new CompileContext());
            
            var engine = new ScriptEngine();
            engine.ExecuteFile("handlebars.js");
            var source = @"
(function(){
var template = new Handlebars.Template();
template.r = " + result.Output + @";
return template.render({world:'Andrew'});
}());";
            var templateRender = engine.Evaluate<string>(source);

            templateRender.ShouldEqual("Hello Andrew");
        }
    }
}