using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Knapsack
{
    public class UnresolvedModuleBuilder
    {
        readonly string rootDirectory;
        readonly string[] scriptFileExtensions = new[] { "js", "coffee" };
        readonly string moduleManifestFilename = "module.txt";

        public UnresolvedModuleBuilder(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public UnresolvedModule Build(string relativeModulePath)
        {
            var modulePath = rootDirectory + relativeModulePath;
            var manifestFilename = modulePath + "/" + moduleManifestFilename;
            IEnumerable<UnresolvedScript> scripts;
            if (File.Exists(manifestFilename))
            {
                scripts = LoadScriptsInManifest(manifestFilename, modulePath);
            }
            else
            {
                scripts = LoadScriptsByFindingFiles(modulePath);
            }

            return new UnresolvedModule(relativeModulePath, scripts.ToArray());
        }

        IEnumerable<UnresolvedScript> LoadScriptsInManifest(string manifestFilename, string modulePath)
        {
            return File.ReadAllLines(manifestFilename)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(filename => modulePath + "/" + filename)
                .Select(NormalizePathSlashes)
                .Select(LoadScript);
        }

        IEnumerable<UnresolvedScript> LoadScriptsByFindingFiles(string modulePath)
        {
            return scriptFileExtensions
                .SelectMany(
                    extension => LoadAllFilesInModule(modulePath, extension)
                )
                .Where(ShouldNotIgnoreScript)
                .Select(NormalizePathSlashes)
                .Select(LoadScript);
        }

        IEnumerable<string> LoadAllFilesInModule(string modulePath, string extension)
        {
            return Directory.EnumerateFiles(
                modulePath, 
                "*." + extension, 
                SearchOption.AllDirectories
            );
        }

        bool ShouldNotIgnoreScript(string filename)
        {
            return !filename.EndsWith("-vsdoc.js");
        }

        string NormalizePathSlashes(string path)
        {
            // For sanity, Knapsack uses the convention of forward slashes everywhere.
            return path.Replace('\\', '/');
        }

        UnresolvedScript LoadScript(string filename)
        {
            var parser = new UnresolvedScriptParser();
            using (var fileStream = File.OpenRead(filename))
            {
                return parser.Parse(fileStream, filename.Substring(rootDirectory.Length));
            }
        }
    }
}
