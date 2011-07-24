using Jurassic;
using Jurassic.Library;

namespace Cassette.Less
{
    public class LessCompiler : ILessCompiler
    {
        public LessCompiler()
        {
            engine = new ScriptEngine();
            engine.Execute("window = {};");
            engine.Execute(Properties.Resources.less);
        }

        readonly ScriptEngine engine;

        public string Compile(string lessSource)
        {
            lock (engine)
            {
                var parser = (ObjectInstance)engine.Evaluate("(new window.less.Parser)");
                var callback = new Callback(engine);
                try
                {
                    parser.CallMemberFunction("parse", lessSource, callback);
                }
                catch (JavaScriptException ex)
                {
                    var message = ((ObjectInstance)ex.ErrorObject).GetPropertyValue("message").ToString();
                    throw new LessCompileException(message);
                }

                if (callback.Css != null)
                {
                    return callback.Css;
                }
                else
                {
                    throw new LessCompileException(callback.ErrorMessage);
                }
            }
        }

        class Callback : FunctionInstance
        {
            public Callback(ScriptEngine engine) : base(engine.Function.InstancePrototype)
            {
            }

            public string Css { get; private set; }
            public string ErrorMessage { get; private set; }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var error = argumentValues[0];
                if (error == Null.Value)
                {
                    var tree = (ObjectInstance)argumentValues[1];
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
