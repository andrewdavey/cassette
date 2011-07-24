using Jurassic;
using Jurassic.Library;
using System;

namespace Cassette.Less
{
    public class LessCompiler : ILessCompiler
    {
        public LessCompiler(Func<string, string> readTextFile)
        {
            this.readTextFile = readTextFile;
            engine = new ScriptEngine();
            engine.Execute("window = {};");
            engine.SetGlobalFunction("loadStyleSheet", new Action<ObjectInstance, FunctionInstance>(LoadStylesheet));
            engine.Execute(Properties.Resources.less);
        }

        readonly Func<string, string> readTextFile;
        readonly ScriptEngine engine;

        public string CompileFile(string lessFilename)
        {
            var result = CompileFileImpl(lessFilename);
            if (result.Css != null)
            {
                return result.Css;
            }
            else
            {
                throw new LessCompileException(string.Format("Less compile error in {0}:\r\n{1}", lessFilename, result.ErrorMessage));
            }
        }

        CompileResult CompileFileImpl(string lessFilename)
        {
            var lessSource = readTextFile(lessFilename);
            lock (engine)
            {
                var parser = (ObjectInstance)engine.Evaluate("(new window.less.Parser)");
                var callback = new CompileResult(engine);
                try
                {
                    parser.CallMemberFunction("parse", lessSource, callback);
                }
                catch (JavaScriptException ex)
                {
                    var message = ((ObjectInstance)ex.ErrorObject).GetPropertyValue("message").ToString();
                    throw new LessCompileException(string.Format("Less compile error in {0}:\r\n{1}", lessFilename, message));
                }

                return callback;
            }
        }

        void LoadStylesheet(ObjectInstance options, FunctionInstance callback)
        {
            var href = options.GetPropertyValue("href").ToString();
            var result = CompileFileImpl(href);
            callback.Call(Null.Value, result.Root);
        }

        class CompileResult : FunctionInstance
        {
            public CompileResult(ScriptEngine engine)
                : base(engine.Function.InstancePrototype)
            {
            }

            public object Root { get; private set; }
            public string Css { get; private set; }
            public string ErrorMessage { get; private set; }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var error = argumentValues[0];
                if (error == Null.Value)
                {
                    var tree = (ObjectInstance)argumentValues[1];
                    Root = tree;
                    Css = tree.CallMemberFunction("toCSS").ToString();
                }
                else
                {
                    ErrorMessage = ((ObjectInstance)error).GetPropertyValue("message").ToString();
                }
                return Undefined.Value;
            }
        }
    }
}
