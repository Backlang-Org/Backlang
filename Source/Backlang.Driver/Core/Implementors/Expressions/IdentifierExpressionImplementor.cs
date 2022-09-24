using Backlang.Contracts.Scoping.Items;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class IdentifierExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsId;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        scope.TryGet<ScopeItem>(node.Name.Name, out var item);

        if (item is ParameterScopeItem psi)
        {
            return block.AppendInstruction(Instruction.CreateLoadArg(psi.Parameter));
        }

        if (item is VariableScopeItem vsi)
        {
            return block.AppendInstruction(Instruction.CreateLoadLocal(vsi.Parameter));
        }

        return null;
    }
}