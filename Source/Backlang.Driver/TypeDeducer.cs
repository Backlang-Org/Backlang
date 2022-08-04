using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Driver.Compiling.Stages.CompilationStages;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver;

public static class TypeDeducer
{
    public static IType Deduce(LNode node, Scope scope, CompilerContext context)
    {
        if (ImplementationStage.LiteralTypeMap.ContainsKey(node.Name))
        {
            return ImplementationStage.GetLiteralType(node, context.Binder);
        }

        if (node.Calls(CodeSymbols.Add) || node.Calls(CodeSymbols.Mul)
            || node.Calls(CodeSymbols.Div)
            || node.Calls(CodeSymbols.Sub))
        {
            return DeduceBinary(node, scope, context);
        }

        if (node.Calls(CodeSymbols._AddressOf))
        {
            var inner = Deduce(node.Args[0], scope, context);

            return inner.MakePointerType(PointerKind.Transient);
        }

        if (node.IsId)
        {
            if (scope.TryGet<ScopeItem>(node.Name.Name, out var item))
            {
                return item.Type;
            }
            else
            {
                context.AddError(node, $"{node.Name} cannot be resolved");
            }
        }

        return null;
    }

    private static IType DeduceBinary(LNode node, Scope scope, CompilerContext context)
    {
        var left = Deduce(node.Args[0], scope, context);
        var right = Deduce(node.Args[1], scope, context);

        if (left != right)
        {
            context.AddError(node, "Type mismatch");
            return null;
        }

        return left;
    }
}