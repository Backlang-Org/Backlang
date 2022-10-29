using Backlang.Contracts.Scoping.Items;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class CallExpressionEmitter : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsCall && !node.Calls(CodeSymbols.Tuple);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (node.Calls(CodeSymbols.New))
        {
            //ToDo: append ctor
        }

        if (scope.TryGet<FunctionScopeItem>(node.Name.Name, out var fn))
        {
            return ImplementationStage.AppendCall(context, block, node, fn.Overloads, scope, modulename, methodName: node.Name.Name);
        }

        context.AddError(node, $"function {node.Name.Name} not found");
        return null;
    }
}