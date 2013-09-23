using System;
using System.Configuration;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

namespace Cassette.Compass
{
    public class CompassPipeline : BundlePipeline<CompassBundle>
    {
        public CompassPipeline(TinyIoCContainer container, CassetteSettings settings) : base(container)
        {
            AddRange(new IBundleProcessor<CompassBundle>[]
            {
                container.Resolve<AssignStylesheetRenderer>(),
                new ParseCssReferences(), 
                new ParseCompassReferences(), 
                new CompileCompass(new CompassCompiler(GetRubyPath(settings)), settings), 
                container.Resolve<ExpandCssUrls>(),
                new SortAssetsByDependency(),
                new AssignHash()
            });

            if (!settings.IsDebuggingEnabled)
            {
                Add(container.Resolve<ConcatenateAssets>());
                var minifier = container.Resolve<IStylesheetMinifier>();
                Add(new MinifyAssets(minifier));
            }
        }

        string GetRubyPath(CassetteSettings settings)
        {
            var settingValue = ConfigurationManager.AppSettings["RubyBinPath"];

            if(string.IsNullOrWhiteSpace(settingValue)) throw new InvalidOperationException("The RubyBinPath appSetting was not defined. Point it to a path that contains ruby.exe and compass.");

            // app-relative path to ruby
            if (settingValue.StartsWith("/") || settingValue.StartsWith("~"))
            {
                var rootDirectory = settings.SourceDirectory as FileSystemDirectory;

                if (rootDirectory == null)
                    throw new InvalidOperationException("Can't use Compass with a non-physical root directory");

                return rootDirectory.GetAbsolutePath(settingValue);
            }
            
            // assuming absolute path to ruby
            return settingValue;
        }
    }
}
