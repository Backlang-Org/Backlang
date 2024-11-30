using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class AsExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node)
    {
        return node.ArgCount == 2 && node.Calls(CodeSymbols.As);
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        //ToDo: if type is obj and expr is valuetype -> box and vice versa unbox
        var exprType = TypeDeducer.Deduce(node.Args[0], scope, context, modulename.Value);

        AppendExpression(block, node.Args[0], exprType, context, scope, modulename);

        var instr = Instruction.CreateDynamicCast(elementType.MakePointerType(PointerKind.Transient), null);

        return block.AppendInstruction(instr);
    }
}