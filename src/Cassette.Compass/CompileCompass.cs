using System;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;

namespace Cassette.Compass
{
    public class CompileCompass : IBundleProcessor<CompassBundle>
    {
        readonly CompassCompiler compassCompiler;
        readonly CassetteSettings settings;

        public CompileCompass(CompassCompiler compassCompiler, CassetteSettings settings)
        {
            this.compassCompiler = compassCompiler;
            this.settings = settings;
        }

        public void Process(CompassBundle bundle)
        {
            // execute compass on the whole project
            compassCompiler.CompileCompass(bundle.Project.ProjectDirectoryPath);

            
            var sassAssets = bundle.Assets.Where(IsSassOrScss).ToArray();

            if (sassAssets.Length == 0) return;

            foreach (var asset in sassAssets)
            {
                var file = asset as FileAsset;
                if (file != null)
                {
                    // because we aren't using IronRuby to invoke Compass, we don't get the very nice PAL-based sass import detection that the normal Cassette SASS compiler gets.
                    // there does not seem to be a good native way around this, so for the moment we'll use a hack: reference ALL sass files that are partials (start with _) that are siblings or children of the sass file.

                    var directory = file.File.Directory;
                    var partials = directory.GetFiles("_*", SearchOption.AllDirectories);
                    foreach (var partial in partials)
                    {
                        asset.AddRawFileReference(partial.FullPath);
                    }
                    
                    // add a faux transformer on each asset that causes it to essentially redirect to the css Compass compiled for it
                    asset.AddAssetTransformer(new CompassAssetTransformer(GetOutputFilePath(file, bundle.Project)));		
                }
                else throw new ArgumentException("Compass bundle contained a non-file asset. This should not occur as Compass only works with files.", "bundle");
            }
        }

        string GetOutputFilePath(FileAsset asset, CompassProject project)
        {
            // TODO: doubt this works
            if (string.IsNullOrWhiteSpace(project.VirtualSassDirectory)) throw new ArgumentException("Compass project did not contain a css_dir that was parseable!");
            if (string.IsNullOrWhiteSpace(project.VirtualCssDirectory)) throw new ArgumentException("Compass project did not contain a sass_dir that was parseable!");

            if (!asset.Path.StartsWith(project.VirtualSassDirectory)) throw new ArgumentException("Asset was outside the sass path!");

            var relativeAssetPath = asset.Path.Substring(project.VirtualSassDirectory.Length);

            var virtualAssetPath = project.VirtualCssDirectory + Path.ChangeExtension(relativeAssetPath, ".css");

            var rootDirectory = settings.SourceDirectory as FileSystemDirectory;

            if (rootDirectory == null) throw new InvalidOperationException("Can't use Compass with a non-physical root directory");

            return rootDirectory.GetAbsolutePath(virtualAssetPath);
        }

        bool IsSassOrScss(IAsset asset)
        {
            var path = asset.Path;
            return path.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".sass", StringComparison.OrdinalIgnoreCase);
        }
    }
}
