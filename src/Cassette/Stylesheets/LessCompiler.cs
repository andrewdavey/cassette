﻿using System;
using System.Collections.Generic;
using Cassette.IO;
using Cassette.Utilities;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Loggers;
using dotless.Core.Parser;

namespace Cassette.Stylesheets
{
    public class LessCompiler : ICompiler
    {
        public string Compile(string source, IFile sourceFile)
        {
            var parser = new Parser
            {
                Importer = new Importer(new CassetteLessFileReader(sourceFile.Directory))
            };
            var errorLogger = new ErrorLogger();
            var engine = new LessEngine(parser, errorLogger, false);
            
            var css = engine.TransformToCss(source, sourceFile.FullPath);

            if (errorLogger.HasErrors)
            {
                throw new LessCompileException(errorLogger.ErrorMessage);
            }
            else
            {
                return css;
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
                get { return string.Join(Environment.NewLine, errors).Trim(); }
            }

            public void Error(string message)
            {
                errors.Add(message);
            }

            public void Log(LogLevel level, string message)
            {
            }

            public void Info(string message)
            {
            }

            public void Debug(string message)
            {
            }

            public void Warn(string message)
            {
            }
        }

        class CassetteLessFileReader : IFileReader
        {
            readonly IDirectory directory;

            public CassetteLessFileReader(IDirectory directory)
            {
                this.directory = directory;
            }

            public string GetFileContents(string fileName)
            {
                return directory.GetFile(fileName).OpenRead().ReadToEnd();
            }

            public bool DoesFileExist(string fileName)
            {
                return directory.GetFile(fileName).Exists;
            }
        }
    }
}