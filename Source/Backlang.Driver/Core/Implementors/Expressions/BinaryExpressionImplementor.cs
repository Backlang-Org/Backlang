using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class BinaryExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.ArgCount == 2 && !node.Calls(CodeSymbols.ColonColon);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, IMethod method, CompilerContext context, Scope scope)
    {
        var lhs = AppendExpression(block, node.Args[0], elementType, method, context, scope);
        var rhs = AppendExpression(block, node.Args[1], elementType, method, context, scope);

        return block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic(node.Name.Name.Substring(1), false, elementType, lhs, rhs));
    }
}