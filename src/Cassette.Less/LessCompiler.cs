using System;
using System.Collections.Generic;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Loggers;
using dotless.Core.Parser;

#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette.Stylesheets
{
    public class LessCompiler : ILessCompiler
    {
        HashedSet<string> importedFilePaths;

        public CompileResult Compile(string source, CompileContext context)
        {
            var sourceFile = context.RootDirectory.GetFile(context.SourceFilePath);
            importedFilePaths = new HashedSet<string>();
            var parser = new Parser
            {
                Importer = new Importer(new CassetteLessFileReader(sourceFile.Directory, importedFilePaths))
            };
            var errorLogger = new ErrorLogger();
            var engine = new LessEngine(parser, errorLogger, false, false);

            string css;
            try
            {
                css = engine.TransformToCss(source, sourceFile.FullPath);
            }
            catch (Exception ex)
            {
                throw new LessCompileException(
                    string.Format("Error compiling {0}{1}{2}", context.SourceFilePath, Environment.NewLine, ex.Message),
                    ex
                );
            }

            if (errorLogger.HasErrors)
            {
                var exceptionMessage = string.Format(
                    "Error compiling {0}{1}{2}",
                    context.SourceFilePath,
                    Environment.NewLine,
                    errorLogger.ErrorMessage
                );
                throw new LessCompileException(exceptionMessage);
            }
            else
            {
                return new CompileResult(css, importedFilePaths);
            }
        }

        class ErrorLogger : ILogger
        {
            readonly List<string> errors;

            public ErrorLogger()
            {
                errors = new List<string>();
            }

            public bool HasErrors
            {
                get { return errors.Count > 0; }
            }

            public string ErrorMessage
            {
                get { return string.Join(Environment.NewLine, errors.ToArray()).Trim(); }
            }

            public void Log(LogLevel level, string message)
            {
                switch (level)
                {
                    case LogLevel.Info: Info(message); break;
                    case LogLevel.Debug: Debug(message); break;
                    case LogLevel.Warn: Warn(message); break;
                    case LogLevel.Error: Error(message); break;
                }
            }

            public void Error(string message)
            {
                errors.Add(message);
                Trace.Source.TraceInformation(message);
            }

            public void Error(string message, params object[] args)
            {
                errors.Add(string.Format(message, args));
                Trace.Source.TraceInformation(message, args);
            }

            public void Info(string message)
            {
                Trace.Source.TraceInformation(message);
            }

            public void Info(string message, params object[] args)
            {
                Trace.Source.TraceInformation(message, args);
            }

            public void Debug(string message)
            {
                Trace.Source.TraceInformation(message);
            }

            public void Debug(string message, params object[] args)
            {
                Trace.Source.TraceInformation(message, args);
            }

            public void Warn(string message)
            {
                Trace.Source.TraceInformation(message);
            }

            public void Warn(string message, params object[] args)
            {
                Trace.Source.TraceInformation(message, args);
            }
        }

        class CassetteLessFileReader : IFileReader
        {
            readonly IDirectory directory;
            readonly HashedSet<string> importFilePaths;

            public CassetteLessFileReader(IDirectory directory, HashedSet<string> importFilePaths)
            {
                this.directory = directory;
                this.importFilePaths = importFilePaths;
            }

            public byte[] GetBinaryFileContents(string fileName)
            {
                var file = directory.GetFile(fileName);
                importFilePaths.Add(file.FullPath);
                using (var buffer = new MemoryStream())
                {
                    using (var fileStream = file.OpenRead())
                    {
                        fileStream.CopyTo(buffer);
                    }
                    return buffer.ToArray();
                }
            }

            public string GetFileContents(string fileName)
            {
                var file = directory.GetFile(fileName);
                importFilePaths.Add(file.FullPath);
                return file.OpenRead().ReadToEnd();
            }

            public bool DoesFileExist(string fileName)
            {
                return directory.GetFile(fileName).Exists;
            }
        }
    }
}