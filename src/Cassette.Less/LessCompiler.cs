using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;
using Trace = Cassette.Diagnostics.Trace;

#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette.Stylesheets
{
    public class LessCompiler : ILessCompiler
    {
        public CompileResult Compile(string source, CompileContext context)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), ".cassette.less");

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            foreach (var file in context.RootDirectory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file.FullPath.Replace("/", "\\"));

                if (!fileName.EndsWith(".less"))
                {
                    continue;
                }

                var fileDirectory = Path.GetDirectoryName(file.FullPath.Replace("/", "\\")).Replace("~", tempPath);

                var tempFile = Path.Combine(fileDirectory, fileName);

                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                using (var s = file.OpenRead())
                {
                    File.WriteAllText(tempFile, s.ReadToEnd());
                }
            }

            var input = context.SourceFilePath.Replace("~", tempPath).Replace("/", "\\"); //  Path.Combine(tempPath, "input.less");
            if (!input.StartsWith(tempPath))
            {
                input = Path.Combine(tempPath, input);
            }

            var output = Path.Combine(tempPath, "output.less");

            var lessjs = Path.Combine(tempPath, "less-1.4.1.min.js");
            var es5shim = Path.Combine(tempPath, "es5-shim.min.js");
            var lessc = Path.Combine(tempPath, "lessc.wsf");

            File.WriteAllText(input, source);

            IList<string> importedFilePaths = new List<string>();

            ProcessImports(importedFilePaths, source, tempPath, context.SourceFilePath);

            File.WriteAllText(lessjs, Resource1.less_1_4_1_min);
            File.WriteAllText(es5shim, Resource1.es5_shim_min);
            File.WriteAllText(lessc, Resource1.lessc);

            var start = new ProcessStartInfo(@"cscript")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Arguments = "//nologo //s \"" + lessc + "\" \"" + input + "\" \"" + output + "\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                WorkingDirectory = tempPath
            };

            var p = new Process
            {
                StartInfo = start, 
                EnableRaisingEvents = true
            };

            p.Start();
            
            p.WaitForExit(10000);
            
            var isSuccess = false;
            var result = string.Empty;
            var error = string.Empty;

            try
            {
                if (File.Exists(output))
                {
                    if (p.ExitCode == 0)
                    {
                        isSuccess = true;
                        result = File.ReadAllText(output);
                    }
                    else
                    {
                        error = p.StandardError.ReadToEnd();
                    }

                    File.Delete(output);
                }
                else
                {
                    error = p.StandardError.ReadToEnd();
                }
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }

            if (!isSuccess)
            {
                tempPath = char.ToLowerInvariant(tempPath[0]) + tempPath.Substring(1);

                var exceptionMessage = string.Format(
                    "Error compiling {0}{1}{2}",
                    context.SourceFilePath,
                    Environment.NewLine,
                    error.Replace(tempPath.Replace('\\', '/'), "~")
                );

                Trace.Source.TraceInformation(error);

                throw new LessCompileException(exceptionMessage);
            }
            else
            {
                return new CompileResult(result, importedFilePaths);
            }
        }

        private static readonly Regex ImportRegex = new Regex(@"@import\s+""(?<import>[\w\.\-_]+)""", RegexOptions.Multiline);

        void ProcessImports(IList<string> importFilePaths, string source, string tempPath, string inputFile)
        {
            if (!inputFile.StartsWith("~/"))
            {
                inputFile = "~/" + inputFile;
            }

            var imports = ImportRegex.Matches(source);
            var relPath = Path.GetDirectoryName(inputFile).Replace('\\', '/');

            foreach (Match import in imports)
            {
                var importFile = import.Groups["import"].Value;
                var relImport = relPath + "/" + importFile;
                
                if (!importFilePaths.Contains(relImport))
                {
                    importFilePaths.Add(relImport);

                    var importSource = File.ReadAllText(relImport.Replace("~", tempPath).Replace('/', '\\'));

                    ProcessImports(importFilePaths, importSource, tempPath, relImport);
                }
            }
        }
    }
}