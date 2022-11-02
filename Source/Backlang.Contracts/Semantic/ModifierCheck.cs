using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts.Semantic;

internal class ModifierCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, CompilerContext context)
    {
        var nodesWithModifiers = tree.Body
            .SelectMany(_ => _.DescendantsAndSelf()).Where(IsModifiableNode).ToArray();

        foreach (var node in nodesWithModifiers)
        {
            CheckForInvalidModifierCombination(node, context);
        }
    }

    private void CheckForInvalidModifierCombination(LNode node, CompilerContext context)
    {
        var attrs = node.Attrs;
        var condition = (attrs.Contains(LNode.Id(CodeSymbols.Public)) && attrs.Contains(LNode.Id(CodeSymbols.Private)))
             || (attrs.Contains(LNode.Id(CodeSymbols.Public)) && attrs.Contains(LNode.Id(CodeSymbols.Internal)))
             || (attrs.Contains(LNode.Id(CodeSymbols.Private)) && attrs.Contains(LNode.Id(CodeSymbols.Internal)));

        if (condition)
        {
            context.AddError(node, ErrorID.InvalidModifierCombination);
        }
    }

    private bool IsModifiableNode(LNode arg)
    {
        return arg.Calls(CodeSymbols.Class) || arg.Calls(CodeSymbols.Struct) || arg.Calls(CodeSymbols.Fn) || arg.Calls(Symbols.Bitfield);
    }
}