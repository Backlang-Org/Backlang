using Furesoft.Core.CodeDom.Compiler.Instructions;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class BinaryExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.ArgCount == 2 && !node.Calls(CodeSymbols.ColonColon) && node.Name.Name.StartsWith("'");

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope)
    {
        var lhs = AppendExpression(block, node.Args[0], elementType, context, scope);
        var rhs = AppendExpression(block, node.Args[1], elementType, context, scope);

        var leftType = TypeDeducer.Deduce(node.Args[0], scope, context);
        var rightType = TypeDeducer.Deduce(node.Args[1], scope, context);

        if (leftType.TryGetOperator(node.Name.Name, out var opMethod, leftType, rightType))
        {
            return block.AppendInstruction(
                Instruction.CreateCall(opMethod, MethodLookup.Static, new ValueTag[] { lhs, rhs }));
        }

        return block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic(node.Name.Name.Substring(1), false, elementType, lhs, rhs));
    }
}