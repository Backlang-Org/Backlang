using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Contracts.Scoping.Items;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class IdentifierExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsId;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope)
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