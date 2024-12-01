using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using BacklangC.Core;
using Loyc.Syntax;

namespace BacklangC.Semantic.Checks;

internal class TypenameCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, Driver context)
    {
        for (var i = 0; i < tree.Body.Count; i++)
        {
            var node = tree.Body[i];

            if (node.Calls(CodeSymbols.Class) || node.Calls(CodeSymbols.Struct))
            {
                if (node is var (_, typename, _) && char.IsLower(typename.Name.Name[0]))
                {
                    context.Messages.Add(
                        Message.Warning($"Type '{typename.Name.Name}' should be Uppercase", node.Range));
                }
            }
        }
    }
}