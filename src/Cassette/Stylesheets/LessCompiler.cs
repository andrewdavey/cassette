using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;
using Jurassic;
using Jurassic.Library;

namespace Cassette.Stylesheets
{
    public class LessCompiler : ICompiler
    {
        static readonly Lazy<ScriptEngine> LazyEngine = new Lazy<ScriptEngine>(CreateScriptEngine);
        readonly Stack<IFile> currentFiles = new Stack<IFile>();

        static ScriptEngine CreateScriptEngine()
        {
            var engine = new ScriptEngine();
            const string stubWindowLocationForCompiler = "window = { location: { href: '/', protocol: 'http:', host: 'localhost' } };";
            engine.Execute(stubWindowLocationForCompiler);
            engine.Execute(Properties.Resources.less);
            return engine;
        }

        static ScriptEngine ScriptEngine
        {
            get { return LazyEngine.Value; }
        }

        void Xhr(ObjectInstance href, ObjectInstance type, FunctionInstance callback, FunctionInstance errorCallback)
        {
            var filename = href.ToString();
            var referencingFile = currentFiles.Peek();
            try
            {
                var file = referencingFile.Directory.GetFile(filename);
                var content = file.OpenRead().ReadToEnd();
                callback.Call(Null.Value, content, DateTime.MinValue);
            }
            catch (FileNotFoundException ex)
            {
                throw FileNotFoundExceptionWithSourceFilename(referencingFile, ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw DirectoryNotFoundExceptionWithSourceFilename(referencingFile, ex);
            }
        }

        public string Compile(string lessSource, IFile sourceFile)
        {
            Trace.Source.TraceInformation("Compiling {0}", sourceFile.FullPath);
            lock (ScriptEngine)
            {
                currentFiles.Clear();
                ScriptEngine.SetGlobalFunction("xhr", new Action<ObjectInstance, ObjectInstance, FunctionInstance, FunctionInstance>(Xhr));

                var result = CompileImpl(lessSource, sourceFile);
                if (result.Css != null)
                {
                    Trace.Source.TraceInformation("Compiled {0}", sourceFile.FullPath);
                    return result.Css;
                }
                else
                {
                    var message = string.Format(
                        "Less compile error in {0}:\r\n{1}", 
                        sourceFile.FullPath, 
                        result.ErrorMessage
                    );
                    Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                    throw new LessCompileException(message);
                }
            }
        }

        CompileResult CompileImpl(string lessSource, IFile file)
        {
            currentFiles.Push(file);

            var parser = (ObjectInstance)ScriptEngine.Evaluate("(new window.less.Parser)");
            var callback = new CompileResult(ScriptEngine);
            try
            {
                parser.CallMemberFunction("parse", lessSource, callback);
            }
            catch (JavaScriptException ex)
            {
                var message = ((ObjectInstance)ex.ErrorObject).GetPropertyValue("message").ToString();
                throw new LessCompileException(
                    string.Format(
                        "Less compile error in {0}:\r\n{1}",
                        file.FullPath,
                        message
                    )
                );
            }
            catch (InvalidOperationException ex)
            {
                throw new LessCompileException(
                    string.Format("Less compile error in {0}.", file.FullPath),
                    ex
                );
            }

            currentFiles.Pop();
            return callback;
        }

        FileNotFoundException FileNotFoundExceptionWithSourceFilename(IFile file, FileNotFoundException ex)
        {
            return new FileNotFoundException(
                string.Format(
                    "{0}{1}Referenced by an @import in '{2}'.",
                    ex.Message,
                    Environment.NewLine,
                    file.FullPath
                ),
                ex
            );
        }

        DirectoryNotFoundException DirectoryNotFoundExceptionWithSourceFilename(IFile file, DirectoryNotFoundException ex)
        {
            return new DirectoryNotFoundException(
                string.Format(
                    "{0}{1}Referenced by an @import in '{2}'.",
                    ex.Message,
                    Environment.NewLine,
                    file.FullPath
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