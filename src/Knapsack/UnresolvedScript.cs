﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Knapsack
{
    /// <summary>
    /// An unresolved script's references are as they appear in the script source.
    /// </summary>
    public class UnresolvedScript
    {
        readonly string path;
        readonly byte[] hash;
        readonly string[] references;

        readonly Regex pathSplitter = new Regex(@"\\|/");

        public UnresolvedScript(string path, byte[] hash, string[] references)
        {
            this.path = path.ToLower();
            this.hash = hash;
            this.references = references;
        }

        public string Path
        {
            get { return path; }
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public string[] References
        {
            get { return references; }
        }

        /// <summary>
        /// Creates a new Script with only module-internal, application relative script references.
        /// The tuple also contains any module-external references used by the script.
        /// </summary>
        public Tuple<Script, string[]> Resolve(Func<string, bool> isInternalPath)
        {
            var applicationRelativeReferences = CreateApplicationRelativeReferences().ToArray();

            var newScript = new Script(
                path,
                hash,
                applicationRelativeReferences.Where(isInternalPath).ToArray()
            );
            var externalReferences = applicationRelativeReferences
                .Where(r => !isInternalPath(r))
                .ToArray();

            return Tuple.Create(newScript, externalReferences);
        }

        IEnumerable<string> CreateApplicationRelativeReferences()
        {
            return ExpandPaths(references, path);
        }

        IEnumerable<string> ExpandPaths(IEnumerable<string> relativePaths, string sourcePath)
        {
            var currentDirectory = System.IO.Path.GetDirectoryName(sourcePath);
            var currentDirectoryNames = pathSplitter.Split(currentDirectory);
            foreach (var path in relativePaths)
            {
                yield return NormalizePath(currentDirectoryNames, path);
            }
        }

        string NormalizePath(string[] currentDirectoryNames, string path)
        {
            // ("app", "~/foo/bar.js") -> "foo/bar.js" 
            if (path.StartsWith("~")) return path.Substring(2);

            // ("app/sub", "../foo/bar.js") -> "app/foo/bar.js"
            var names = currentDirectoryNames.Concat(pathSplitter.Split(path));
            var stack = new Stack<string>();
            foreach (var name in names)
            {
                if (name == "..")
                {
                    if (stack.Count == 0) throw new Exception("Path cannot ascend above the website's root directory.");
                    stack.Pop();
                }
                else
                {
                    stack.Push(name);
                }
            }
            return string.Join("\\", stack.Reverse());
        }


    }
}