using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.BundleProcessing;
using Cassette.Stylesheets;

namespace Cassette.Compass
{
	public class CompassBundleFactory : BundleFactoryBase<CompassBundle>
	{
		readonly Func<IBundlePipeline<CompassBundle>> compassPipeline;

		public CompassBundleFactory(Func<IBundlePipeline<CompassBundle>> compassPipeline)
		{
			this.compassPipeline = compassPipeline;
		}

		protected override CompassBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
		{
			return new CompassBundle(path) { Pipeline = compassPipeline() };
		}
	}
}
