using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cassette.RequireJS
{
    public class AmdConfiguration : List<AmdModule>
    {
        public void Add(string assetPath, string amdModulePath, string alias)
        {
            Add(new AmdModule
            {
                AssetPath = assetPath,
                ModulePath = amdModulePath,
                Alias = alias
            });
        }

        public string GetModulePathForAsset(string toPath)
        {
            var module = this.FirstOrDefault(m => m.AssetPath == toPath);
            if (module == null) return RequireJsPath(toPath);
            return module.ModulePath;
        }

        string RequireJsPath(string assetPath)
        {
            var path = assetPath.Substring(2);
            return RemoveFileExtension(path);
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

        public string GetModuleVariableName(string assetPath)
        {
            var module = this.FirstOrDefault(m => m.AssetPath == assetPath);
            if (module == null) return ExportedVariableName(assetPath);
            return module.Alias;
        }

        string ExportedVariableName(string assetPath)
        {
            var name = Path.GetFileNameWithoutExtension(assetPath);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }
    }

    public class AmdModule
    {
        public string AssetPath { get; set; }
        public string ModulePath { get; set; }
        public string Alias { get; set; }
    }
}