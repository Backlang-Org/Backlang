namespace Backlang.Driver.Core.Implementors.Expressions;

public class AddressExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node is ("'&", _);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (node is (_, var p))
        {
            var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == p.Name.Name);
            if (localPrms.Any())
            {
                return block.AppendInstruction(Instruction.CreateLoadLocalAdress(new Parameter(localPrms.First().Type, localPrms.First().Tag.Name)));
            }
        }

        return null;
    }
}