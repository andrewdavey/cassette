using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    class FileAccessAuthorization : IFileAccessAuthorization
    {
        readonly BundleCollection bundles;
        readonly HashedCompareSet<string> paths = new HashedCompareSet<string>(new string[0], StringComparer.OrdinalIgnoreCase);
        readonly List<Func<string, bool>> pathPredicates = new List<Func<string, bool>>();

        public FileAccessAuthorization(IEnumerable<IConfiguration<IFileAccessAuthorization>> configurations, BundleCollection bundles)
        {
            this.bundles = bundles;
            ApplyConfigurations(configurations);
        }

        void ApplyConfigurations(IEnumerable<IConfiguration<IFileAccessAuthorization>> configurations)
        {
            configurations.OrderByConfigurationOrderAttribute().Configure(this);
        }

        public void AllowAccess(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (!path.StartsWith("~/")) throw new ArgumentException(string.Format("The path \"{0}\" is not application relative. It must start with \"~/\".", path), "path");
            
            paths.Add(path);
        }

        public void AllowAccess(Func<string, bool> pathPredicate)
        {
            if (pathPredicate == null) throw new ArgumentNullException("pathPredicate");
            
            pathPredicates.Add(pathPredicate);
        }

        public bool CanAccess(string path)
        {
            if (path == null) return false;

            return 
                paths.Contains(path) || 
                pathPredicates.Any(predicate => predicate(path)) || 
                BundlesContainRawFileReference(path);
        }

        bool BundlesContainRawFileReference(string path)
        {
            using (bundles.GetReadLock())
            {
                return RawFileReferenceFinder.RawFileReferenceExists(path, bundles);
            }
        }
    }
}