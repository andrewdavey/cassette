using System.Runtime.InteropServices;
using Should;
using Xunit;

namespace Cassette.Interop
{
    public class IEJavaScriptEngine_Tests
    {
        [Fact]
        public void GivenLibraryLoaded_WhenCallFunctionPassingInArgument_ThenResultReturned()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.Initialize();
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
                engine.Initialize();
                engine.LoadLibrary(Scripts.Properties.Resources.coffeescript);
                engine.LoadLibrary("function compile(code) { return CoffeeScript.compile(code); }");
                var js = engine.CallFunction<string>("compile", "x = 1");
                js.ShouldEqual("(function() {\n  var x;\n\n  x = 1;\n\n}).call(this);\n");
            }
        }

        [Fact]
        public void GivenSyntaxError_WhenLoadLibrary_ThenExceptionThrown()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.Initialize();
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
                engine.Initialize();
                engine.LoadLibrary("function fail() { return this.x.y; }");
                Assert.Throws<ActiveScriptException>(
                    () => engine.CallFunction<object>("fail")
                );
            }
        }

        [Fact]
        public void GivenGlobalAddedToEngine_WhenScriptUsesGlobalFunction_ThenResultReturned()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.Initialize();
                engine.AddGlobalValue("Test", new GlobalData());
                engine.LoadLibrary("function go() { return Test.hello('John'); }");
                var result = engine.CallFunction<string>("go");
                result.ShouldEqual("Hello, John");
            }
        }

        [Fact]
        public void GivenGlobalAddedToEngine_WhenScriptUsesGlobalFunctionWithCallback_ThenCallbackInvoked()
        {
            using (var engine = new IEJavaScriptEngine())
            {
                engine.Initialize();
                engine.AddGlobalValue("Test", new GlobalData());
                engine.LoadLibrary(@"
function go() { 
    var result;
    Test.process(function(x) {
        result = x;
    });
    return result;
}");
                // TODO: Someone smarter can figure out how to make this test pass in FX35. (kamranayub)
#if NET40
                var result = engine.CallFunction<string>("go");
                result.ShouldEqual("done");
#endif
            }
        }

        [ComVisible(true)]
        public class GlobalData
        {
            public string hello(string name)
            {
                return "Hello, " + name;
            }
#if NET40
            public void process(dynamic callback)
            {
                // By the magic of 'dynamic' this actually figures out the correct COM-voodoo required to call the callback!!
                callback("done");                
            }
#endif
        }
    }
}