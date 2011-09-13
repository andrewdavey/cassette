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
        readonly Stack<IFile> currentFiles = new Stack<IFile>();

        public string Compile(string lessSource, IFile sourceFile)
        {
            lock (engine)
            {
                currentFiles.Clear();

                var result = CompileImpl(lessSource, sourceFile);
                if (result.Css != null)
                {
                    return result.Css;
                }
                else
                {
                    throw new LessCompileException(
                        string.Format(
                            "Less compile error in {0}:\r\n{1}", 
                            sourceFile.FullPath, 
                            result.ErrorMessage
                        )
                    );
                }
            }
        }

        CompileResult CompileImpl(string lessSource, IFile file)
        {
            currentFiles.Push(file);
            
            var parser = (ObjectInstance)engine.Evaluate("(new window.less.Parser)");
            var callback = new CompileResult(engine);
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
                    ),
                    ex
                );
            }
            catch (Exception ex)
            {
                throw new LessCompileException(
                    string.Format("Less compile error in {0}.", file.FullPath),
                    ex
                );
            }

            currentFiles.Pop();
            return callback;
        }

        void LoadStylesheet(ObjectInstance options, FunctionInstance callback)
        {
            var href = options.GetPropertyValue("href").ToString();
            var referencingFile = currentFiles.Peek();
            string source;
            IFile file;
            try
            {
                file = referencingFile.Directory.GetFile(href);
                using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read)))
                {
                    source = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException ex)
            {
                throw FileNotFoundExceptionWithSourceFilename(referencingFile, ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw DirectoryNotFoundExceptionWithSourceFilename(referencingFile, ex);
            }
            var result = CompileImpl(source, file);
            callback.Call(Null.Value, result.Root);
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
