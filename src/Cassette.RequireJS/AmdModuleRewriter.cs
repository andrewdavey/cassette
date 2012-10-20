using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    public class AmdModuleRewriter
    {
        public string InsertModulePathIntoDefineCall(string moduleScript, string modulePath)
        {
            var parser = new JSParser(moduleScript);
            var tree = parser.Parse(new CodeSettings());
            tree.Accept(new Visitor(modulePath));

            return tree.ToCode();
        }

        class Visitor : TreeVisitor
        {
            readonly string modulePath;

            public Visitor(string modulePath)
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