using Backlang.Driver.Core.Instructions;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class TypeOfExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.ArgCount == 1 && node is ("'typeof", _);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var type = TypeDeducer.Deduce(node.Args[0].Args[0].Args[0], scope, context, modulename.Value);

        return block.AppendInstruction(new TypeOfInstructionPrototype(type).Instantiate(new List<ValueTag>()));
    }
}