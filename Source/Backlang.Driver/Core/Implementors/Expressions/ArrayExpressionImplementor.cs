using Furesoft.Core.CodeDom.Compiler.Core.Constants;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class ArrayExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.Array);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block, IType elementType,
        CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var value = block.AppendInstruction(Instruction.CreateConstant(new IntegerConstant(node.ArgCount), context.Environment.Int32));
        var counter = block.AppendInstruction(Instruction.CreateLoad(context.Environment.Int32, value));

        if (elementType.FullName.Qualifier is GenericName gn)
        {
            elementType = context.Binder.ResolveTypes(gn.TypeArgumentNames[0]).FirstOrDefault();
        }

        //ToDo: add array initialisation
        return block.AppendInstruction(Instruction.CreateAllocaArray(elementType, counter));
    }
}