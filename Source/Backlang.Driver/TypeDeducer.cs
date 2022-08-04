using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Driver.Compiling.Stages.CompilationStages;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver;

public static class TypeDeducer
{
    //ToDo: check for implicit cast
    public static IType Deduce(LNode node, Scope scope, CompilerContext context)
    {
        if (ImplementationStage.LiteralTypeMap.ContainsKey(node.Name))
        {
            return ImplementationStage.GetLiteralType(node, context.Binder);
        }
        else if (node.ArgCount == 1)
        {
            if (node.Calls(CodeSymbols._AddressOf))
            {
                var inner = Deduce(node.Args[0], scope, context);

                return inner.MakePointerType(PointerKind.Transient);
            }
            else if (node.Calls(CodeSymbols.Mul))
            {
                var t = Deduce(node.Args[0], scope, context);

                if (t is PointerType pt)
                {
                    return pt.ElementType;
                }

                context.AddError(node, "Cannot dereference non pointer type");
            }
        }
        else if (node.ArgCount == 2)
        {
            if (node.Calls(CodeSymbols.Add) || node.Calls(CodeSymbols.Mul)
                || node.Calls(CodeSymbols.Div) || node.Calls(CodeSymbols.Sub))
            {
                return DeduceBinary(node, scope, context);
            }
            else if (node.Calls(CodeSymbols.LT)
                || node.Calls(CodeSymbols.GT) || node.Calls(CodeSymbols.LE)
                || node.Calls(CodeSymbols.GE))
            {
                NotExpectType(node[0], scope, context, context.Environment.Boolean);
                NotExpectType(node[1], scope, context, context.Environment.Boolean);

                return context.Environment.Boolean;
            }
            else if (node.Calls(CodeSymbols.Eq) || node.Calls(CodeSymbols.NotEq))
            {
                return context.Environment.Boolean;
            }
        }
        else if (node.IsId)
        {
            if (scope.TryGet<ScopeItem>(node.Name.Name, out var item))
            {
                return item?.Type;
            }
            else
            {
                context.AddError(node, $"{node.Name} cannot be resolved");
            }
        }

        return null;
    }

    public static void ExpectType(LNode node, Scope scope, CompilerContext context, IType expectedType)
    {
        var deducedType = Deduce(node, scope, context);

        if (deducedType != expectedType)
        {
            context.AddError(node, $"Type Mismatch. Expected {expectedType}, got {deducedType}");
        }
    }

    public static void NotExpectType(LNode node, Scope scope, CompilerContext context, IType expectedType)
    {
        var deducedType = Deduce(node, scope, context);

        if (deducedType == expectedType)
        {
            context.AddError(node, $"{expectedType} is not allowed here");
        }
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