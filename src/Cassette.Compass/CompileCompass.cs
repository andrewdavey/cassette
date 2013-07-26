using System;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;

namespace Cassette.Compass
{
	public class CompileCompass : IBundleProcessor<CompassBundle>
	{
		readonly CompassCompiler compassCompiler;
		readonly CassetteSettings settings;

		public CompileCompass(CompassCompiler compassCompiler, CassetteSettings settings)
		{
			this.compassCompiler = compassCompiler;
		}

		public void Process(CompassBundle bundle)
		{
			// execute compass on the whole project
			compassCompiler.CompileCompass(bundle.CompassProjectPath);

			
			var sassAssets = bundle.Assets.Where(IsSassOrScss).ToArray();

			if (sassAssets.Length == 0) return;

			var project = new CompassProjectParser(bundle.CompassProjectPath).Parse();

			foreach (var asset in sassAssets)
			{
				var file = asset as FileAsset;
				if (file != null)
				{
					// because we aren't using IronRuby to invoke Compass, we don't get the very nice PAL-based sass import detection that the normal Cassette SASS compiler gets.
					// there does not seem to be a good native way around this, so for the moment we'll use a hack: reference ALL sass files that are partials (start with _) that are siblings or children of the sass file.

					var directory = Path.GetDirectoryName(file.Path);
					var partials = Directory.GetFiles(directory, "_*", SearchOption.AllDirectories);
					foreach (var partial in partials)
					{
						asset.AddRawFileReference(partial.Replace(directory, string.Empty));
					}
				}

				// add a faux transformer on each asset that causes it to essentially redirect to the css Compass compiled for it
				asset.AddAssetTransformer(new CompassAssetTransformer(GetOutputFilePath(asset, project)));			
			}
		}

		string GetOutputFilePath(IAsset asset, CompassProject project)
		{
			if (string.IsNullOrWhiteSpace(project.CssDirectory)) throw new ArgumentException("Compass project did not contain a css_dir that was parseable!");
			if (string.IsNullOrWhiteSpace(project.SassDirectory)) throw new ArgumentException("Compass project did not contain a sass_dir that was parseable!");

			var compassRootPath = Path.GetDirectoryName(project.ProjectFilePath);
			var path = asset.Path.Replace(compassRootPath, string.Empty).Trim(Path.DirectorySeparatorChar);

			var normalizedCssDir = project.CssDirectory.Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar);
			var normalizedSassDir = project.SassDirectory.Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar);

			if(path.StartsWith(normalizedSassDir))
				return compassRootPath + Path.DirectorySeparatorChar + normalizedCssDir + Path.ChangeExtension(Path.GetFileName(path), ".css");

			return asset.Path;
		}

		bool IsSassOrScss(IAsset asset)
		{
			var path = asset.Path;
			return path.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ||
				   path.EndsWith(".sass", StringComparison.OrdinalIgnoreCase);
		}
	}
}
