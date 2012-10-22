using System;
using System.Collections.Generic;
using System.IO;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    public class ShimAmdModule : IAmdModule, IAssetTransformer
    {
        readonly string moduleReturnExpression;
        readonly IEnumerable<string> dependencies;
        readonly IJsonSerializer jsonSerializer;

        public ShimAmdModule(IAsset asset, Bundle bundle, string moduleReturnExpression, IEnumerable<string> dependencies, IJsonSerializer jsonSerializer)
        {
            this.moduleReturnExpression = moduleReturnExpression;
            this.dependencies = dependencies;
            this.jsonSerializer = jsonSerializer;

            Asset = asset;
            Bundle = bundle;
            ScriptPath = asset.Path;
            ModulePath = PathHelpers.ConvertCassettePathToModulePath(asset.Path);
            Alias = Path.GetFileNameWithoutExtension(asset.Path);
        }

        public IAsset Asset { get; private set; }
        public Bundle Bundle { get; private set; }
        public string ScriptPath { get; private set; }
        public string ModulePath { get; set; }
        public string Alias { get; set; }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var source = reader.ReadToEnd();
                    var output = string.Format(
                        "{0}\r\ndefine({1},{2},function(){{return {3};}});",
                        source,
                        jsonSerializer.Serialize(ModulePath),
                        jsonSerializer.Serialize(dependencies),
                        moduleReturnExpression
                    );
                    return output.AsStream();
                }
            };
        }
    }
}