using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class PointerExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node)
    {
        return node is ("'*", _) && node.ArgCount == 1;
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (node is var (_, o))
        {
            var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == o.Name.Name);
            if (localPrms.Any())
            {
                AppendExpression(block, o, elementType, context, scope, modulename);
                return block.AppendInstruction(Instruction.CreateLoadIndirect(localPrms.First().Type));
            }
        }

        return null;
    }
}