using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Contracts.Scoping.Items;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class IdentifierExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsId;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope)
    {
        scope.TryGet<FunctionScopeItem>(node.Name.Name, out var fn);

        var par = fn.Method.Parameters.Where(_ => _.Name.ToString() == node.Name.Name);

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