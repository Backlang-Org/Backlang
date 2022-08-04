using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class PointerExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node is ("'*", _) && node.ArgCount == 1;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, IMethod method, CompilerContext context, Scope scope)
    {
        if (node is (_, var o))
        {
            var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == o.Name.Name);
            if (localPrms.Any())
            {
                AppendExpression(block, o, elementType, method, context, scope);
                return block.AppendInstruction(Instruction.CreateLoadIndirect(localPrms.First().Type));
            }
        }

        return null;
    }
}