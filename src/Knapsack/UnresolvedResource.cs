using System;
using System.Collections.Generic;
using System.Linq;

namespace Knapsack
{
    /// <summary>
    /// An unresolved resources's references as they appear in the file.
    /// </summary>
    public class UnresolvedResource
    {
        readonly string path;
        readonly byte[] hash;
        readonly string[] references;

        public UnresolvedResource(string path, byte[] hash, string[] references)
        {
            this.path = path;
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
        /// Creates a new Resource with only module-internal, application relative references.
        /// The tuple also contains any module-external references used by the resource.
        /// </summary>
        public Tuple<Resource, string[]> Resolve(Func<string, bool> isInternalPath)
        {
            var applicationRelativeReferences = CreateApplicationRelativeReferences().ToArray();

            var newScript = new Resource(
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
            var parts = sourcePath.Split('/');
            var currentDirectoryNames = parts.Take(parts.Length - 1).ToArray();
            foreach (var path in relativePaths)
            {
                yield return NormalizePath(currentDirectoryNames, path);
            }
        }

        string NormalizePath(string[] currentDirectoryNames, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("The path cannot be empty.", "path");

            // ("app", "~/foo/bar.js") -> "foo/bar.js" 
            if (path.StartsWith("~"))
            {
                if (path.Length == 1) throw new ArgumentException("The path \"~\" is missing a file name.", "path");
                path = path.Substring(2);
            }

            // ("app/sub", "../foo/bar.js") -> "app/foo/bar.js"
            var names = currentDirectoryNames.Concat(path.Split('/'));
            var stack = new Stack<string>();
            foreach (var name in names)
            {
                if (name == "..")
                {
                    if (stack.Count == 0) throw new ArgumentException("The path cannot ascend above the website's root directory. Too many \"..\" in the path \"" + path + "\".", "path");
                    stack.Pop();
                }
                else
                {
                    stack.Push(name);
                }
            }
            return string.Join("/", stack.Reverse());
        }
    }
}
