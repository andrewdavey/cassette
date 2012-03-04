using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cassette.IO;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using SassAndCoffee.Core;
using SassAndCoffee.Ruby.Sass;

namespace Cassette.Stylesheets
{
    /// <remarks>
    /// Based on the SassCompiler from SassAndCoffee.Ruby, but modified to work with
    /// Cassette's file system abstraction.
    /// </remarks>
    public class SassCompiler : ICompiler
    {
        ScriptEngine engine;
        ScriptScope scope;
        PlatformAdaptationLayer pal;
        dynamic sassCompiler;
        dynamic sassOption;
        dynamic scssOption;
        bool initialized;
        readonly object _lock = new object();
        IDirectory rootDirectory;

        public string Compile(string source, IFile sourceFile)
        {
            var dependentFileList = new List<string>();

            lock (_lock)
            {
                rootDirectory = sourceFile.Directory;
                Initialize();

                var compilerOptions = GetCompilerOptions(sourceFile);

                // Removed this because "current directory" doesn't exist for Cassette' file system abstraction yet.
                // The CassettePlatformAdaptationLayer handles this for us instead.
                //var directoryPath = pathInfo.DirectoryName;
                //if (!directoryPath.Contains("\'"))
                //{
                //    var statement = String.Format("Dir.chdir '{0}'", directoryPath);
                //    _engine.Execute(statement, _scope);
                //}

                // TODO: Reintroduce this once the Cassette PAL supports OnOpenInputFileStream.
                //if (dependentFileList != null)
                //{
                //    dependentFileList.Add(sourceFile.FullPath);
                //    _pal.OnOpenInputFileStream = (accessedFile) =>
                //    {
                //        if (!accessedFile.Contains(".sass-cache"))
                //            dependentFileList.Add(accessedFile);
                //    };
                //}

                string result;
                try
                {
                    result = (string)sassCompiler.compile(source, compilerOptions);
                }
                catch (Exception e)
                {
                    // Provide more information for SassSyntaxErrors
                    if (e.Message == "Sass::SyntaxError")
                    {
                        throw CreateSassSyntaxError(sourceFile, e);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    // TODO: Reintroduce this once the Cassette PAL supports OnOpenInputFileStream.
                    //_pal.OnOpenInputFileStream = null;
                }
                return result;
            }
        }

        dynamic GetCompilerOptions(IFile sourceFile)
        {
            var extension = Path.GetExtension(sourceFile.FullPath);
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException(string.Format("Cannot determine the file extension from the filename \"{0}\". Expected \".sass\" or \".scss\".", sourceFile.FullPath));
            }

            var isSass = extension.Equals(".sass", StringComparison.OrdinalIgnoreCase);
            return isSass ? sassOption : scssOption;
        }

        Exception CreateSassSyntaxError(IFile sourceFile, Exception originalException)
        {
            dynamic rubyError = originalException;
            var message = new StringBuilder();
            message.AppendFormat("{0}\n\n", rubyError.to_s());
            message.AppendFormat("Backtrace:\n{0}\n\n", rubyError.sass_backtrace_str(sourceFile.FullPath) ?? "");
            message.AppendFormat("FileName: {0}\n\n", rubyError.sass_filename() ?? sourceFile.FullPath);
            message.AppendFormat("MixIn: {0}\n\n", rubyError.sass_mixin() ?? "");
            message.AppendFormat("Line Number: {0}\n\n", rubyError.sass_line() ?? "");
            message.AppendFormat("Sass Template:\n{0}\n\n", rubyError.sass_template ?? "");
            return new Exception(message.ToString(), originalException);
        }

        void Initialize()
        {
            if (initialized) return;

            pal = new CassettePlatformAdaptationLayer(new ResourceRedirectionPlatformAdaptationLayer(), RootDirectory);
            var srs = new ScriptRuntimeSetup
            {
                HostType = typeof(SassCompilerScriptHost),
                HostArguments = new List<object> { pal },
            };
            srs.AddRubySetup();
            var runtime = Ruby.CreateRuntime(srs);
            engine = runtime.GetRubyEngine();

            // NB: 'R:\{345ED29D-C275-4C64-8372-65B06E54F5A7}' is a garbage path that the PAL override will 
            // detect and attempt to find via an embedded Resource file
            engine.SetSearchPaths(new List<string> 
            { 
                @"R:\{345ED29D-C275-4C64-8372-65B06E54F5A7}\lib\ironruby",
                @"R:\{345ED29D-C275-4C64-8372-65B06E54F5A7}\lib\ruby\1.9.1"
            });

            var source = engine.CreateScriptSourceFromString(
                Utility.ResourceAsString("lib.sass_in_one.rb", typeof(SassAndCoffee.Ruby.Sass.SassCompiler)),
                SourceCodeKind.File);
            scope = engine.CreateScope();
            source.Execute(scope);

            sassCompiler = scope.Engine.Runtime.Globals.GetVariable("Sass");
            sassOption = engine.Execute("{:cache => false, :syntax => :sass}");
            scssOption = engine.Execute("{:cache => false, :syntax => :scss}");

            initialized = true;
        }

        IDirectory RootDirectory()
        {
            return rootDirectory;
        }
    }
}