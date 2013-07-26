namespace Cassette.Compass
{
	public class CompassProject
	{
		public CompassProject(string projectFilePath, string projectDirectoryPath)
		{
			ProjectFilePath = projectFilePath;
			ProjectDirectoryPath = projectDirectoryPath;
		}

		public string ProjectFilePath { get; private set; }
		public string ProjectDirectoryPath { get; private set; }

		public string PhysicalCssDirectory { get; set; }
		public string VirtualCssDirectory { get; set; }
		public string PhysicalSassDirectory { get; set; }
		public string VirtualSassDirectory { get; set; }
	}
}
