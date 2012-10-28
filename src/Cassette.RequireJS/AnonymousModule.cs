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

        public string Transform(string source, IAsset asset)
        {
            return InsertModulePathIntoDefineCall(source, ModulePath);
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