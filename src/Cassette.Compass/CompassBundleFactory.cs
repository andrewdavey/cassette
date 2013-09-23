using System;
using System.Collections.Generic;
using System.IO;
using Cassette.BundleProcessing;
using Cassette.IO;

namespace Cassette.Compass
{
    public class CompassBundleFactory : IBundleFactory<CompassBundle>
    {
        readonly Func<IBundlePipeline<CompassBundle>> compassPipeline;
        private readonly CassetteSettings settings;

        public CompassBundleFactory(Func<IBundlePipeline<CompassBundle>> compassPipeline, CassetteSettings settings)
        {
            this.settings = settings;
            this.compassPipeline = compassPipeline;
        }

        public CompassBundle CreateBundle(string path, IEnumerable<IFile> allFiles, BundleDescriptor bundleDescriptor)
        {
            var rootDirectory = settings.SourceDirectory as FileSystemDirectory;

            if(rootDirectory == null) throw new InvalidOperationException("Can't use Compass with a non-physical root directory");

            var absolutePath = rootDirectory.GetAbsolutePath(path);

            var project = new CompassProjectParser(absolutePath, path).Parse();

            var result = new CompassBundle(path, project);

            result.Pipeline = compassPipeline();

            AddAssets(result, project.VirtualSassDirectory, allFiles);
            
            return result;
        }

        private void AddAssets(CompassBundle bundle, string virtualSassPath, IEnumerable<IFile> allFiles)
        {
            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileName(file.FullPath);

                if (!file.Directory.FullPath.StartsWith(virtualSassPath)) continue; // ignore sass files outside the sasspath
            
                if (fileName.StartsWith("_")) continue; // ignore partials

                string extension = Path.GetExtension(fileName);

                if(extension == ".scss" || extension == ".sass")
                    bundle.Assets.Add(new FileAsset(file, bundle));
            }
        }
    }
}
