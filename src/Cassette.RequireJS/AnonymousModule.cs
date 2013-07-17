using System;
using System.IO;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    class AnonymousModule : Module, IAssetTransformer
    {
        public AnonymousModule(IAsset asset, Bundle bundle,string baseUrl = null)
            : base(asset, bundle, baseUrl)
        {
            asset.AddAssetTransformer(this);
        }
    
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                var source = openSourceStream().ReadToEnd();
                var output = ModulePathInserter.InsertModulePathIntoDefineCall(source, ModulePath);
                return output.AsStream();
            };
        }

        /// <summary>
        /// Finds the anonymous module "define" call and inserts the module path
        /// as the first argument.
        /// </summary>
        class ModulePathInserter : TreeVisitor
        {
            public static string InsertModulePathIntoDefineCall(string moduleScript, string modulePath)
            {
                var inserter = new ModulePathInserter();
                var parser = new JSParser(moduleScript);
                var sourceTree = parser.Parse(new CodeSettings { MinifyCode = false });
                sourceTree.Accept(inserter);
                if (inserter.insertionIndex > 0)
                {
                    return moduleScript.Insert(inserter.insertionIndex, "\"" + modulePath + "\",");
                }
                else
                {
                    return moduleScript;
                }
            }

            int insertionIndex;

            public override void Visit(CallNode node)
            {
                base.Visit(node);

                if (ShouldInsertModulePathArgument(node))
                {
                    insertionIndex = node.Arguments[0].Context.StartPosition;
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