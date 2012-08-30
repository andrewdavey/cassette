using System;
using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Precompiled
{
    public class CustomServicesConfiguration : ServicesConfiguration
    {
        public CustomServicesConfiguration()
        {
            UrlModifierType = typeof (CdnUrlModifier);
        }
    }

    public class BundlesConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<StylesheetBundle>("Content");
            bundles.Add<ScriptBundle>("Scripts");
        }
    }

    public class FileAuthConfiguration : IConfiguration<IFileAccessAuthorization>
    {
        public void Configure(IFileAccessAuthorization authorization)
        {
            authorization.AllowAccess(path => path.StartsWith("~/content", StringComparison.OrdinalIgnoreCase));
        }
    }
}