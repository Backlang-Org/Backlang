using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class BinaryExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.ArgCount == 2;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block, IType elementType, IMethod method)
    {
        var lhs = AppendExpression(block, node.Args[0], elementType, method);
        var rhs = AppendExpression(block, node.Args[1], elementType, method);

        return block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic(node.Name.Name.Substring(1), false, elementType, lhs, rhs));
    }
}
