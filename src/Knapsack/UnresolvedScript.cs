using System;
using System.Collections.Generic;
using System.Linq;

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
            var directory = System.IO.Path.GetDirectoryName(path);
            return references.Select(r => System.IO.Path.Combine(directory, r));
        }

    }
}
