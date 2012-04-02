using System.Collections.Generic;
using Cassette.Configuration;
using Cassette.IO;
using System;
using TinyIoC;

namespace Cassette.MSBuild
{
    public class MSBuildBootstrapper : DefaultBootstrapperBase
    {
        public string OutputFilename { get; set; }

        protected override CassetteSettings Settings
        {
            get
            {
                var settings = base.Settings;
                settings.SourceDirectory = new FileSystemDirectory(Environment.CurrentDirectory);
                return settings;
            }
        }

        protected override Type UrlModifier
        {
            get
            {
                return typeof(UrlPlaceholderWrapper);
            }
        }

        protected override void RegisterTypesAsSingletons(IEnumerable<TypeRegistration> typeRegistrations, TinyIoCContainer container)
        {
            base.RegisterTypesAsSingletons(typeRegistrations, container);

            container.Register(
                (c, p) => new CreateBundlesImplementation(
                    OutputFilename,
                    c.Resolve<BundleCollection>(),
                    c.Resolve<CassetteSettings>()
                )
            );
        }
    }
}