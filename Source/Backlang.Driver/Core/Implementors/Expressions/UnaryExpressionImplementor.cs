using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class UnaryExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.ArgCount == 1 && node.Args[0].HasValue;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, IMethod method, CompilerContext context, Scope scope)
    {
        var constant = ConvertConstant(elementType, node.Args[0].Value);
        var value = block.AppendInstruction(constant);

        return block.AppendInstruction(Instruction.CreateLoad(elementType, value));
    }
}