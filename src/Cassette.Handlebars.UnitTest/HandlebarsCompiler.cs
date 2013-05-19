using System.Collections.Generic;
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
            var settings = new HandlebarsSettings { KnownHelpersOnly = false, KnownHelpers = new List<string> { "t" } };

            var compiler = new HandlebarsCompiler(settings);
            var result = compiler.Compile("Hello {{world}}", new CompileContext());
            
            var engine = new ScriptEngine();
            engine.ExecuteFile("handlebars.js");
            var source = @"
(function(){
var template = new Hogan.template(" + result.Output + @");
return template({world:'Andrew'});
}());";
            var templateRender = engine.Evaluate<string>(source);

            templateRender.ShouldEqual("Hello Andrew");
        }
    }
}