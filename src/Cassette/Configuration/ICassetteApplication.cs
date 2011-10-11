using System.Collections.Generic;
using Cassette.IO;

namespace Cassette.Configuration
{
    public interface ICassetteApplicationC
    {
        IList<Bundle> Bundles { get; }
        IUrlGenerator UrlGenerator { get; set; }
        bool OptimizationEnabled { get; set; }
        bool HtmlRewritingEnabled { get; set; }
    }

    public interface ICassetteConfiguration
    {
        void Configure(ICassetteApplicationC application);
    }

    public static class BundleExtensions
    {
        public static void AddFile(this Bundle bundle, string virtualPath)
        {
            Cassette.ICassetteApplication application;

            //var file = application.RootDirectory.GetFile(virtualPath);
            //bundle.Assets.Add(new Asset(virtualPath, bundle, file));
        }
    }

}
