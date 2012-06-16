using Jurassic;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HoganCompiler_Tests
    {
        [Fact]
        public void CanCompileHoganTemplate()
        {
            var compiler = new HoganCompiler();
            var result = compiler.Compile("Hello {{world}}", new CompileContext());
            
            var engine = new ScriptEngine();
            engine.ExecuteFile("hogan.js");
            var source = @"
(function(){
var template = new Hogan.Template();
template.r = " + result.Output + @";
return template.render({world:'Andrew'});
}());";
            var templateRender = engine.Evaluate<string>(source);

            templateRender.ShouldEqual("Hello Andrew");
        }
    }
}