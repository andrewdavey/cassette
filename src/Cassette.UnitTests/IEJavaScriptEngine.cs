using Cassette.Interop;
using Should;
using Xunit;

namespace Cassette
{
    public class IEJavaScriptEngine_Tests
    {
        [Fact]
        public void GivenLibraryLoaded_WhenCallFunctionPassingInArgument_ThenResultReturned()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.LoadLibrary("function test(input) { return input * 2; }");
                var result = engine.CallFunction<int>("test", 10);
                result.ShouldEqual(20);
            }
        }

        [Fact]
        public void GivenCoffeeScriptCompilerLoaded_WhenCompileCalled_ThenJavaScriptReturned()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.LoadLibrary(Cassette.Properties.Resources.coffeescript);
                engine.LoadLibrary("function compile(code) { return CoffeeScript.compile(code); }");
                var js = engine.CallFunction<string>("compile", "x = 1");
                js.ShouldEqual("(function() {\n  var x;\n  x = 1;\n}).call(this);\n");
            }
        }

        [Fact]
        public void GivenSyntaxError_WhenLoadLibrary_ThenExceptionThrown()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                Assert.Throws<ActiveScriptException>(
                    () => engine.LoadLibrary("var !x = 1;")
                );
            }
        }

        [Fact]
        public void GivenScriptThatThrows_WhenExecuted_ThenExceptionThrown()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.LoadLibrary("function fail() { return this.x.y; }");
                Assert.Throws<ActiveScriptException>(
                    () => engine.CallFunction<object>("fail")
                );
            }
        }

        [Fact]
        public void GivenCoffeeScriptCompilerLoaded_WhenCompileInvalidCoffeeScript_ThenExceptionThrown()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.LoadLibrary(Cassette.Properties.Resources.coffeescript);
                engine.LoadLibrary("function compile(code) { return CoffeeScript.compile(code); }");
                var exception = Assert.Throws<ActiveScriptException>(
                    () => engine.CallFunction<string>("compile", "x = [1..")
                );
                exception.Message.ShouldEqual("unclosed [ on line 1");
            }
        }
    }
}