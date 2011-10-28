using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    abstract class BundleFactoryBase<T> : IBundleFactory<T> 
        where T : Bundle
    {
        public abstract T CreateBundle(string pathOrUrl);

        public virtual T CreateBundle(string path, IEnumerable<IFile> allFiles, BundleDescriptor bundleDescriptor)
        {
            var bundle = CreateBundleCore(path, bundleDescriptor);
            AddAssets(bundle, allFiles, bundleDescriptor);
            AddReferences(bundle, bundleDescriptor.References);
            SetIsSortedIfExplicitFilenames(bundle, bundleDescriptor.AssetFilenames);
            return bundle;
        }

        protected abstract T CreateBundleCore(string path, BundleDescriptor bundleDescriptor);

        void AddAssets(Bundle bundle, IEnumerable<IFile> allFiles, BundleDescriptor bundleDescriptor)
        {
            var remainingFiles = new HashSet<IFile>(allFiles);
            var filesByPath = allFiles.ToDictionary(f => f.FullPath);

            foreach (var filename in bundleDescriptor.AssetFilenames)
            {
                if (filename == "*")
                {
                    foreach (var file in remainingFiles)
                    {
                        bundle.Assets.Add(new Asset(bundle, file));
                    }
                    break;
                }
                else
                {
                    IFile file;
                    if (filesByPath.TryGetValue(filename, out file))
                    {
                        bundle.Assets.Add(new Asset(bundle, file));
                        remainingFiles.Remove(file);
                    }
                    else
                    {
                        throw new FileNotFoundException(string.Format("The asset file \"{0}\" was not found for bundle \"{1}\".", filename, bundle.Path));
                    }
                }
            }
        }

        void AddReferences(Bundle bundle, IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                bundle.AddReference(reference);
            }
        }

        void SetIsSortedIfExplicitFilenames(Bundle bundle, IList<string> filenames)
        {
            if (filenames.Count == 0 || filenames[0] != "*")
            {
                bundle.IsSorted = true;
            }
        }
    }
}