using System;
using System.IO;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    class AnonymousModule : Module, IAssetTransformer
    {
        public AnonymousModule(IAsset asset, Bundle bundle) 
            : base(asset, bundle)
        {
            asset.AddAssetTransformer(this);
        }
    
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
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
            var sourceTree = parser.Parse(new CodeSettings());
            sourceTree.Accept(new ModulePathInserter(modulePath));

            return sourceTree.ToCode();
        }

        /// <summary>
        /// Finds the anonymous module "define" call and inserts the module path
        /// as the first argument.
        /// </summary>
        class ModulePathInserter : TreeVisitor
        {
            readonly string modulePath;

            public ModulePathInserter(string modulePath)
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