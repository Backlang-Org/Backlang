using Backlang.Codeanalysis.Core;
using Backlang.Driver;

namespace Backlang.Contracts.Semantic;

internal class TypenameCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, CompilerContext context)
    {
        for (var i = 0; i < tree.Body.Count; i++)
        {
            var node = tree.Body[i];

            if (node.Calls(CodeSymbols.Class) || node.Calls(CodeSymbols.Struct))
            {
                if (node is (_, var typename, _) && char.IsLower(typename.Name.Name[0]))
                {
                    context.Messages.Add(Message.Warning($"Type '{typename.Name.Name}' should be Uppercase", node.Range));
                }
            }
        }
    }
}