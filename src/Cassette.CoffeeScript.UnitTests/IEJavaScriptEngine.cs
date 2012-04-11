using Cassette.Interop;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class IEJavaScriptEngine_Tests
    {
        [Fact]
        public void GivenCoffeeScriptCompilerLoaded_WhenCompileCalled_ThenJavaScriptReturned()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.Initialize();
                engine.LoadLibrary(Properties.Resources.coffeescript);
                engine.LoadLibrary("function compile(code) { return CoffeeScript.compile(code); }");
                var js = engine.CallFunction<string>("compile", "x = 1");
                js.ShouldEqual("(function() {\n  var x;\n\n  x = 1;\n\n}).call(this);\n");
            }
        }
    }
}