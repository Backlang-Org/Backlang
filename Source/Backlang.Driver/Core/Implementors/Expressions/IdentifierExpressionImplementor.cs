using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class IdentifierExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsId;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, IMethod method, CompilerContext context, Scope scope)
    {
        var par = method.Parameters.Where(_ => _.Name.ToString() == node.Name.Name);

        if (!par.Any())
        {
            var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == node.Name.Name);
            if (localPrms.Any())
            {
                return block.AppendInstruction(Instruction.CreateLoadLocal(new Parameter(localPrms.First().Type, localPrms.First().Tag.Name)));
            }
        }
        else
        {
            return block.AppendInstruction(Instruction.CreateLoadArg(par.First()));
        }

        return null;
    }
}