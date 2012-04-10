using System;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.MSBuild
{
    public class MSBuildHost : HostBase
    {
        public string OutputFilename { get; set; }

        public void Execute()
        {
            Container.Resolve<CreateBundlesImplementation>().Execute();
        }

        protected override CassetteSettings Settings
        {
            get
            {
                var settings = base.Settings;
                settings.SourceDirectory = new FileSystemDirectory(Environment.CurrentDirectory);
                return settings;
            }
        }

        protected override void RegisterContainerItems()
        {
            Container.Register(
                (c, p) => new CreateBundlesImplementation(
                    OutputFilename,
                    c.Resolve<BundleCollection>(),
                    c.Resolve<CassetteSettings>()
                )
            );

            base.RegisterContainerItems();

            // Override any URL modifier, even if set by the application.
            // So this is *after* the base.RegisterContainerItems() call.
            // We must output specially wrapped URLs at compile-time. These are then modified by the application at run-time.
            Container.Register<IUrlModifier>(new UrlPlaceholderWrapper());
        }
    }
}