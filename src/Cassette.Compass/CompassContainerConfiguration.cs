using System;
using System.Collections.Generic;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

namespace Cassette.Compass
{
	[ConfigurationOrder(15)]
	public class CompassContainerConfiguration : ContainerConfiguration<CompassBundle>
	{
		public CompassContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes)
			: base(getImplementationTypes)
		{
		}

		public override void Configure(TinyIoCContainer container)
		{
			base.Configure(container);
			container.Register(typeof(IStylesheetMinifier), typeof(MicrosoftStylesheetMinifier));
		}

		protected override string FilePattern
		{
			get { return "*.scss"; }
		}

		protected override Type BundleFactoryType
		{
			get { return typeof(CompassBundleFactory); }
		}

		protected override Type BundlePipelineType
		{
			get { return typeof(CompassPipeline); }
		}
	}
}