using System;
using IOPath = System.IO.Path;
using Cassette.BundleProcessing;
using Cassette.Stylesheets;

namespace Cassette.Compass
{
	public class CompassBundle : StylesheetBundle
	{
		public CompassBundle(string compassProjectPath)	: base(GetSassDirectory(compassProjectPath))
		{
			CompassProjectPath = compassProjectPath;
		}

		public string CompassProjectPath { get; private set; }

		private static string GetSassDirectory(string compassProjectPath)
		{
			var configDirectory = new CompassProjectParser(compassProjectPath).Parse().SassDirectory;
			if(string.IsNullOrWhiteSpace(configDirectory)) throw new ArgumentException("Invalid or unparseable sass directory in " + compassProjectPath);

			return IOPath.Combine(IOPath.GetDirectoryName(compassProjectPath), configDirectory.Replace('/', IOPath.DirectorySeparatorChar));
		}

		public new IBundlePipeline<CompassBundle> Pipeline { get; set; }
	}
}
