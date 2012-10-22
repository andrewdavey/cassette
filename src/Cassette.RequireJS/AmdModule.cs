using System;
using System.IO;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    public class AmdModule : IAmdModule, IAssetTransformer
    {
        public AmdModule(IAsset asset, Bundle bundle, string alias)
        {
            Asset = asset;
            Bundle = bundle;
            Alias = alias;
            ModulePath = GetModulePath(asset);
        }

        string GetModulePath(IAsset asset)
        {
            var source = asset.OpenStream().ReadToEnd();
            string modulePath;
            if (ExplicitModulePathFinder.TryGetModulePath(source, out modulePath))
            {
                return modulePath;
            }

            return PathHelpers.ConvertCassettePathToModulePath(asset.Path);
        }

        public IAsset Asset { get; private set; }
        
        public Bundle Bundle { get; private set; }
        
        public string ModulePath { get; set; }
        
        public string Alias { get; set; }

        public string ScriptPath
        {
            get { return Asset.Path; }
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset _)
        {
            return () =>
            {
                var source = openSourceStream().ReadToEnd();
                var output = InsertModulePathIntoDefineCall(source, ModulePath);
                return output.AsStream();
            };
        }

        string InsertModulePathIntoDefineCall(string moduleScript, string modulePath)
        {
            var parser = new JSParser(moduleScript);
            var tree = parser.Parse(new CodeSettings());
            tree.Accept(new DefineCallModifier(modulePath));

            return tree.ToCode();
        }

        class ExplicitModulePathFinder : TreeVisitor
        {
            public static bool TryGetModulePath(string moduleScript, out string modulePath)
            {
                var parser = new JSParser(moduleScript);
                var tree = parser.Parse(new CodeSettings());
                var finder = new ExplicitModulePathFinder();
                tree.Accept(finder);
                modulePath = finder.ModulePath;
                return finder.ModulePath != null;
            }

            public override void Visit(CallNode node)
            {
                if (IsDefineFunction(node) && node.Arguments.Count > 0)
                {
                    var constWrapper = node.Arguments[0] as ConstantWrapper;
                    if (constWrapper != null && constWrapper.PrimitiveType == PrimitiveType.String)
                    {
                        ModulePath = (string)constWrapper.Value;
                    }
                }
                base.Visit(node);
            }

            public string ModulePath { get; private set; }

            bool IsDefineFunction(CallNode node)
            {
                return node.Function.ToCode() == "define";
            }
        }

        class DefineCallModifier : TreeVisitor
        {
            readonly string modulePath;

            public DefineCallModifier(string modulePath)
            {
                this.modulePath = modulePath;
            }

            public override void Visit(CallNode node)
            {
                base.Visit(node);

                if (ShouldInsertModulePathArgument(node))
                {
                    var modulePathNode = new ConstantWrapper(modulePath, PrimitiveType.String, node.Context, node.Parser);
                    node.Arguments.Insert(0, modulePathNode);
                }
            }

            bool ShouldInsertModulePathArgument(CallNode node)
            {
                return IsDefineFunction(node)
                    && DefineCallIsAnonymous(node);
            }

            bool IsDefineFunction(CallNode node)
            {
                return node.Function.ToCode() == "define";
            }

            bool DefineCallIsAnonymous(CallNode node)
            {
                // e.g. define( [...], function() {} )
                // or   define( function() {} )
                return node.Arguments.Count < 3;
            }
        }
    }
}