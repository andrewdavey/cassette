using Cassette.BundleProcessing;
using Cassette.Stylesheets;

namespace Cassette.Compass
{
	public class CompassBundle : StylesheetBundle
	{
		public CompassBundle(string path, CompassProject compassProject)	: base(path)
		{
			Project = compassProject;
		}

		public CompassProject Project { get; private set; }

		public new IBundlePipeline<CompassBundle> Pipeline { get; set; }

		protected override void ProcessCore(CassetteSettings settings)
		{
			Pipeline.Process(this);
		}
	}
}
