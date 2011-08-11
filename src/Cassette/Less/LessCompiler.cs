using System;
using System.Collections.Generic;
using System.IO;
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
            engine.SetGlobalFunction("loadStyleSheet", new Action<ObjectInstance, FunctionInstance>(LoadStylesheet));
            engine.Execute(Properties.Resources.less);
        }

        readonly ScriptEngine engine;
        readonly Stack<IFileSystem> currentDirectories = new Stack<IFileSystem>();

        public string Compile(string lessSource, string sourceFilename, IFileSystem fileSystem)
        {
            lock (engine)
            {
                var result = CompileImpl(lessSource, sourceFilename, fileSystem);
                if (result.Css != null)
                {
                    return result.Css;
                }
                else
                {
                    throw new LessCompileException(
                        string.Format(
                            "Less compile error in {0}:\r\n{1}", 
                            sourceFilename, 
                            result.ErrorMessage
                        )
                    );
                }
            }
        }

        CompileResult CompileImpl(string lessSource, string sourceFilename, IFileSystem fileSystem)
        {
            currentDirectories.Push(fileSystem);
            
            var parser = (ObjectInstance)engine.Evaluate("(new window.less.Parser)");
            var callback = new CompileResult(engine);
            try
            {
                parser.CallMemberFunction("parse", lessSource, callback);
            }
            catch (JavaScriptException ex)
            {
                var message = ((ObjectInstance)ex.ErrorObject).GetPropertyValue("message").ToString();
                throw new LessCompileException(string.Format("Less compile error in {0}:\r\n{1}", sourceFilename, message));
            }

            currentDirectories.Pop();
            return callback;
        }

        void LoadStylesheet(ObjectInstance options, FunctionInstance callback)
        {
            var href = options.GetPropertyValue("href").ToString();
            var directory = currentDirectories.Peek();
            string source;
            using (var reader = new StreamReader(directory.OpenFile(href, FileMode.Open, FileAccess.Read)))
            {
                source = reader.ReadToEnd();
            }
            var newDirectory = directory.AtSubDirectory(Path.GetDirectoryName(href), false);
            var result = CompileImpl(source, href, newDirectory);
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
