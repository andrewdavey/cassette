using System.Collections.Generic;
using System;

namespace Cassette.Configuration
{
    // ICassetteApplication application
    // application.Bundles...
    // application.Services.Register<ICompiler>("CoffeeScript", () => new CoffeeScriptCompiler());
    // application.Services.Register<IUrlGenerator>(() => new UrlGenerator());
    // application.Settings.Optimized = true;
    // application.Settings.RewriteHtml = true;

    public class CassetteServiceRegistrar
    {
        public void Register<T>(Func<T> getInstance) { }
        public void Register<T>(string name, Func<T> getInstance) { }
    }

    public interface ICassetteApplicationC
    {
        IList<Bundle> Bundles { get; }
        CassetteServiceRegistrar Services { get; }
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
