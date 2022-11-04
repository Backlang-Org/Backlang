using Backlang.Contracts.Scoping.Items;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class MemberExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.ArgCount == 2
        && !node.Calls(CodeSymbols.ColonColon)
        && !node.Calls(CodeSymbols.Tuple)
        && node.Name.Name.StartsWith("'.");

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        scope.TryGet<VariableScopeItem>(node.Args[0].Name.Name, out var item);

        var type = TypeDeducer.Deduce(node.Args[0], scope, context, modulename.Value);

        var field = type.Fields.FirstOrDefault(_ => _.Name.ToString() == node.Args[1].Name.ToString());
        if (field != null)
        {
            block.AppendInstruction(Instruction.CreateLoadLocal(item.Parameter));
            return block.AppendInstruction(Instruction.CreateLoadField(field));
        }

        return null;
    }
}