using Backlang.Contracts.Scoping.Items;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class IdentifierExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsId;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (!scope.TryGet<ScopeItem>(node.Name.Name, out var item))
        {
            context.AddError(node, new(Codeanalysis.Core.ErrorID.NotDefined, node.Name.Name));
            return null;
        }

        if (item is ParameterScopeItem psi)
        {
            return block.AppendInstruction(Instruction.CreateLoadArg(psi.Parameter));
        }
        else if (item is VariableScopeItem vsi)
        {
            return block.AppendInstruction(Instruction.CreateLoadLocal(vsi.Parameter));
        }

        return null;
    }
}