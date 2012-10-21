using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    public class DefineCallTransformer : IAssetTransformer
    {
        readonly AmdConfiguration configuration;
        readonly IJsonSerializer jsonSerializer;

        public DefineCallTransformer(AmdConfiguration configuration, IJsonSerializer jsonSerializer)
        {
            this.configuration = configuration;
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

                    var output = JavaScriptContainsTopLevelVar(source, export)
                        ? ModuleWithReturn(path, dependencyPaths, dependencyAliases, source, export)
                        : ModuleWithoutReturn(path, dependencyPaths, dependencyAliases, source);

                    return output.AsStream();
                }
            };
        }

        static string ModuleWithoutReturn(string path, string dependencyPaths, string dependencyAliases, string source)
        {
            return string.Format(
                "define({0},{1},function({2}){{{3}\r\n}});",
                path,
                dependencyPaths,
                dependencyAliases,
                source
            );
        }

        static string ModuleWithReturn(string path, string dependencyPaths, string dependencyAliases, string source, string export)
        {
            Diagnostics.Trace.Source.TraceInformation("AMD module {0} does not return a value.", path);
            return string.Format(
                "define({0},{1},function({2}){{{3}\r\nreturn {4};}});",
                path,
                dependencyPaths,
                dependencyAliases,
                source,
                export
            );
        }

        IEnumerable<string> DependencyPaths(IAsset asset)
        {
            return asset
                .References
                .Select(reference => configuration[reference.ToPath].ModulePath);
        }

        IEnumerable<string> DependencyAliases(IAsset asset)
        {
            return asset.References.Select(reference => configuration[reference.ToPath].Alias);
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