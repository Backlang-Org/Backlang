using Backlang.Driver.Core.Instructions;
using Furesoft.Core.CodeDom.Compiler.Instructions;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class TypeOfExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node)
    {
        return node.ArgCount == 1 && node is ("'typeof", _);
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var type = TypeDeducer.Deduce(node.Args[0].Args[0].Args[0], scope, context, modulename.Value);

        var ldtoken = block.AppendInstruction(new TypeOfInstructionPrototype(type).Instantiate(new List<ValueTag>()));

        var typeRef = Utils.ResolveType(context.Binder, typeof(Type));
        var method = typeRef.Methods.FirstOrDefault(_ => _.Name.ToString() == "GetTypeFromHandle");

        return block.AppendInstruction(Instruction.CreateCall(method, MethodLookup.Static,
            new List<ValueTag> { ldtoken }));
    }
}