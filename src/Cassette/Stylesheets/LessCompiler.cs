using System;
using System.Collections.Generic;
using System.IO;
using Cassette.IO;
using Jurassic;
using Jurassic.Library;

namespace Cassette.Stylesheets
{
    public class LessCompiler : ICompiler
    {
        public LessCompiler()
        {
            engine = new ScriptEngine();
            engine.Execute("window = {};");
            engine.SetGlobalFunction("loadStyleSheet", new Action<ObjectInstance, FunctionInstance>(LoadStylesheet));
            engine.Execute(Properties.Resources.less);
        }

        readonly ScriptEngine engine;
        readonly Stack<IDirectory> currentDirectories = new Stack<IDirectory>();
        readonly Stack<string> currentFilenames = new Stack<string>();

        public string Compile(string lessSource, string sourceFilename, IDirectory fileSystem)
        {
            lock (engine)
            {
                currentDirectories.Clear();
                currentFilenames.Clear();

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

        CompileResult CompileImpl(string lessSource, string sourceFilename, IDirectory fileSystem)
        {
            currentFilenames.Push(fileSystem.GetAbsolutePath(sourceFilename));
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

            currentFilenames.Pop();
            currentDirectories.Pop();
            return callback;
        }

        void LoadStylesheet(ObjectInstance options, FunctionInstance callback)
        {
            var href = options.GetPropertyValue("href").ToString();
            var currentDirectory = currentDirectories.Peek();
            string source;
            try
            {
                using (var reader = new StreamReader(currentDirectory.OpenFile(href, FileMode.Open, FileAccess.Read)))
                {
                    source = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException ex)
            {
                throw FileNotFoundExceptionWithSourceFilename(ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw DirectoryNotFoundExceptionWithSourceFilename(ex);
            }
            var newDirectory = currentDirectory.NavigateTo(Path.GetDirectoryName(href), false);
            var result = CompileImpl(source, href, newDirectory);
            callback.Call(Null.Value, result.Root);
        }

        FileNotFoundException FileNotFoundExceptionWithSourceFilename(FileNotFoundException ex)
        {
            return new FileNotFoundException(
                string.Format(
                    "{0}{1}Referenced by an @import in '{2}'.",
                    ex.Message,
                    Environment.NewLine,
                    currentFilenames.Peek()
                ),
                ex
            );
        }

        DirectoryNotFoundException DirectoryNotFoundExceptionWithSourceFilename(DirectoryNotFoundException ex)
        {
            return new DirectoryNotFoundException(
                string.Format(
                    "{0}{1}Referenced by an @import in '{2}'.",
                    ex.Message,
                    Environment.NewLine,
                    currentFilenames.Peek()
                ),
                ex
            );
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
