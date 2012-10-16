using System;
using System.IO;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    public class DefineCallTransformer : IAssetTransformer
    {
        readonly IJsonSerializer jsonSerializer;

        public DefineCallTransformer(IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var source = reader.ReadToEnd();
                    var export = ExportedVariablename(asset);
                    var path = RequireJsPath(asset);
                    var output = string.Format(
                        "define({0},[],function(){{{1}\r\nreturn {2};}});",
                        jsonSerializer.Serialize(path),
                        source,
                        export
                        );
                    return output.AsStream();
                }
            };
        }

        static string ExportedVariablename(IAsset asset)
        {
            var name = Path.GetFileNameWithoutExtension(asset.Path);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }

        static string RequireJsPath(IAsset asset)
        {
            var path = asset.Path.Substring(2);
            path = RemoveFileExtension(path);
            return path;
        }

        static string RemoveFileExtension(string path)
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