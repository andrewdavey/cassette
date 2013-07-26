namespace Cassette.Compass
{
	internal class CompassProject
	{
		public CompassProject(string projectFilePath)
		{
			ProjectFilePath = projectFilePath;
		}

		public string ProjectFilePath { get; private set; }
		public string CssDirectory { get; set; }
		public string SassDirectory { get; set; }
	}
}
