using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Compiling.Stages.CompilationStages;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class CallExpressionEmitter : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.IsCall;

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope)
    {
        if (scope.TryGet<FunctionScopeItem>(node.Name.Name, out var fn))
        {
            return ImplementationStage.AppendCall(context, block, node, fn.Method.ParentType.Methods, scope, node.Name.Name);
        }

        context.AddError(node, $"function {node.Name.Name} not found");
        return null;
    }
}