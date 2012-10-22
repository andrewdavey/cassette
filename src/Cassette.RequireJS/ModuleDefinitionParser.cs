using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Traverses a JavaScript source tree looking for a "define" call.
    /// </summary>
    class ModuleDefinitionParser : TreeVisitor
    {
        public bool FoundModuleDefinition { get; private set; }
        
        public string ModulePath { get; private set; }

        public override void Visit(CallNode node)
        {
            if (IsDefineFunction(node) && node.Arguments.Count > 0)
            {
                FoundModuleDefinition = true;

                // If first argument is a string, then it is the module path.
                var constWrapper = node.Arguments[0] as ConstantWrapper;
                if (constWrapper != null && constWrapper.PrimitiveType == PrimitiveType.String)
                {
                    ModulePath = (string)constWrapper.Value;
                }

                return;
            }

            base.Visit(node);
        }

        bool IsDefineFunction(CallNode node)
        {
            return node.Function.ToCode() == "define";
        }
    }
}