using System;
using System.IO;

namespace Cassette.Compass
{
	internal class CompassProjectParser
	{
		readonly string compassProjectPath;

		public CompassProjectParser(string compassProjectPath)
		{
			if (!File.Exists(compassProjectPath))
				throw new ArgumentException("Compass project path did not exist!", "compassProjectPath");

			this.compassProjectPath = compassProjectPath;
		}

		public CompassProject Parse()
		{
			var lines = File.ReadAllLines(compassProjectPath);
			var result = new CompassProject(compassProjectPath);

			foreach (var line in lines)
			{
				if (line.StartsWith("css_dir"))
					result.CssDirectory = line.Substring(line.IndexOf('"') + 1).TrimEnd('"');

				if(line.StartsWith("sass_dir"))
					result.SassDirectory = line.Substring(line.IndexOf('"') + 1).TrimEnd('"');

				if (!string.IsNullOrEmpty(result.SassDirectory) && !string.IsNullOrEmpty(result.CssDirectory))
					break;
			}

			return result;
		}
	}
}
