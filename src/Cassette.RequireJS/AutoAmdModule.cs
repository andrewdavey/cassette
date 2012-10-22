using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Converts plain JavaScript source into an AMD module. The script should contain a top-level
    /// variable that matches the file name of the script. This variable will be returned as the
    /// module's value from the generated <code>define</code> call.
    /// </summary>
    public class AutoAmdModule : IAmdModule, IAssetTransformer
    {
        readonly IJsonSerializer jsonSerializer;
        readonly IAmdModuleCollection modules;

        public AutoAmdModule(IAsset asset, Bundle bundle, IJsonSerializer jsonSerializer, IAmdModuleCollection modules)
        {
            this.jsonSerializer = jsonSerializer;
            this.modules = modules;
            Asset = asset;
            Bundle = bundle;
            ModulePath = PathHelpers.ConvertCassettePathToModulePath(asset.Path);
            Alias = ConvertFilenameToAlias(asset.Path);
        }

        public IAsset Asset { get; private set; }
        
        public Bundle Bundle { get; private set; }

        public string Alias { get; set; }

        public string ModulePath { get; set; }
        
        public string ScriptPath
        {
            get { return Asset.Path; }
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset _)
        {
            return () =>
            {
                var source = openSourceStream().ReadToEnd();
                var referencePaths = Asset.References.Select(r => r.ToPath).ToArray();

                var dependencyPaths = DependencyPaths(referencePaths);
                var dependencyAliases = DependencyAliases(referencePaths);

                var output = JavaScriptContainsTopLevelVar(source, Alias)
                    ? ModuleWithReturn(ModulePath, dependencyPaths, dependencyAliases, source, Alias)
                    : ModuleWithoutReturn(ModulePath, dependencyPaths, dependencyAliases, source);

                return output.AsStream();
            };
        }

        string ConvertFilenameToAlias(string assetPath)
        {
            var name = Path.GetFileNameWithoutExtension(assetPath);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }

        string ModuleWithoutReturn(string path, IEnumerable<string> dependencyPaths, IEnumerable<string> dependencyAliases, string source)
        {
            return string.Format(
                "define({0},{1},function({2}){{{3}\r\n}});",
                jsonSerializer.Serialize(path),
                jsonSerializer.Serialize(dependencyPaths),
                string.Join(",", dependencyAliases),
                source
            );
        }

        string ModuleWithReturn(string path, IEnumerable<string> dependencyPaths, IEnumerable<string> dependencyAliases, string source, string export)
        {
            Diagnostics.Trace.Source.TraceInformation("AMD module {0} does not return a value.", path);
            return string.Format(
                "define({0},{1},function({2}){{{3}\r\nreturn {4};}});",
                jsonSerializer.Serialize(path),
                jsonSerializer.Serialize(dependencyPaths),
                string.Join(",", dependencyAliases),
                source,
                export
            );
        }

        IEnumerable<string> DependencyPaths(IEnumerable<string> referencePaths)
        {
            return referencePaths.Select(p => modules[p].ModulePath);
        }

        IEnumerable<string> DependencyAliases(IEnumerable<string> referencePaths)
        {
            return referencePaths.Select(p => modules[p].Alias);
        }

        bool JavaScriptContainsTopLevelVar(string source, string var)
        {
            var parser = new JSParser(source);
            var tree = parser.Parse(new CodeSettings());
            var finder = new TopLevelVarFinder(var);
            tree.Accept(finder);
            return finder.Found;
        }

        class TopLevelVarFinder : TreeVisitor
        {
            readonly string varName;

            public TopLevelVarFinder(string varName)
            {
                this.varName = varName;
            }

            public override void Visit(VariableDeclaration node)
            {
                if (node.EnclosingScope is GlobalScope && node.Identifier == varName)
                {
                    Found = true;
                }

                base.Visit(node);
            }

            public bool Found { get; private set; }
        }
    }
}