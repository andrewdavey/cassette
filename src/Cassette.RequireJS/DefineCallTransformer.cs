using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    public class DefineCallTransformer : IAssetTransformer
    {
        readonly AmdConfiguration amdConfiguration;
        readonly IJsonSerializer jsonSerializer;

        public DefineCallTransformer(AmdConfiguration amdConfiguration, IJsonSerializer jsonSerializer)
        {
            this.amdConfiguration = amdConfiguration;
            this.jsonSerializer = jsonSerializer;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var source = reader.ReadToEnd();
                    var path = RequireJsPath(asset.Path);
                    var dependencyPaths = jsonSerializer.Serialize(DependencyPaths(asset));
                    var dependencyAliases = string.Join(",", DependencyAliases(asset));
                    var export = ExportedVariableName(asset.Path);

                    var output = string.Format(
                        "define({0},{1},function({2}){{{3}\r\nreturn {4};}});",
                        path,
                        dependencyPaths,
                        dependencyAliases,
                        source,
                        export
                    );
                    return output.AsStream();
                }
            };
        }

        IEnumerable<string> DependencyPaths(IAsset asset)
        {
            return asset
                .References
                .Select(reference => amdConfiguration.GetModulePathForAsset(reference.ToPath));
        }

        IEnumerable<string> DependencyAliases(IAsset asset)
        {
            return asset.References.Select(reference => amdConfiguration.GetModuleVariableName(reference.ToPath));
        }

        string ExportedVariableName(string assetPath)
        {
            var name = Path.GetFileNameWithoutExtension(assetPath);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }

        string RequireJsPath(string assetPath)
        {
            var path = assetPath.Substring(2);
            path = RemoveFileExtension(path);
            return jsonSerializer.Serialize(path);
        }

        string RemoveFileExtension(string path)
        {
            var index = path.LastIndexOf('.');
            if (index >= 0)
            {
                path = path.Substring(0, index);
            }
            return path;
        }
    }
}