using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Driver.Compiling.Stages.CompilationStages;
using Backlang.Driver.Core;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc;
using Loyc.Syntax;
using System.Collections.Immutable;

namespace Backlang.Driver;

public static class TypeDeducer
{
    public static readonly ImmutableDictionary<string, Symbol> TypenameTable = new Dictionary<string, Symbol>()
    {
        ["obj"] = CodeSymbols.Object,
        ["none"] = CodeSymbols.Void,

        ["bool"] = CodeSymbols.Bool,

        ["u8"] = CodeSymbols.UInt8,
        ["u16"] = CodeSymbols.UInt16,
        ["u32"] = CodeSymbols.UInt32,
        ["u64"] = CodeSymbols.UInt64,

        ["i8"] = CodeSymbols.Int8,
        ["i16"] = CodeSymbols.Int16,
        ["i32"] = CodeSymbols.Int32,
        ["i64"] = CodeSymbols.Int64,

        ["f16"] = Symbols.Float16,
        ["f32"] = Symbols.Float32,
        ["f64"] = Symbols.Float64,

        ["char"] = CodeSymbols.Char,
        ["string"] = CodeSymbols.String,
    }.ToImmutableDictionary();

    //ToDo: check for implicit cast
    public static IType Deduce(LNode node, Scope scope, CompilerContext context)
    {
        if (ImplementationStage.LiteralTypeMap.ContainsKey(node.Name))
        {
            return ImplementationStage.GetLiteralType(node, context, scope);
        }
        if (TypenameTable.ContainsKey(node.Name.Name))
        {
            return Deduce(LNode.Id(TypenameTable[node.Name.Name]), scope, context);
        }
        else if (node.ArgCount == 1 && node.Calls(CodeSymbols.Default))
        {
            if (node is (_, (_, (_, var type))))
            {
                return Deduce(type, scope, context);
            }
        }
        else if (node.Calls(CodeSymbols.New))
        {
            if (node is (_, var call))
            {
                return Deduce(call.Target, scope, context);
            }
        }
        else if (node.ArgCount == 1 && node.Name.Name.StartsWith("'"))
        {
            return DeduceUnary(node, scope, context);
        }
        else if (node.ArgCount == 2 && node.Name.Name.StartsWith("'"))
        {
            return DeduceBinary(node, scope, context);
        }
        else if (node.IsId || node.IsCall)
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

    public static IType NotExpectType(LNode node, Scope scope, CompilerContext context, IType expectedType)
    {
        var deducedType = Deduce(node, scope, context);

        if (deducedType == expectedType)
        {
            context.AddError(node, $"{expectedType} is not allowed here");
        }

        return deducedType;
    }

    private static IType DeduceBinary(LNode node, Scope scope, CompilerContext context)
    {
        if (node.Calls(CodeSymbols.Add) || node.Calls(CodeSymbols.Mul)
                || node.Calls(CodeSymbols.Div) || node.Calls(CodeSymbols.Sub) || node.Calls(CodeSymbols.AndBits)
                || node.Calls(CodeSymbols.OrBits) || node.Calls(CodeSymbols.Xor) || node.Calls(CodeSymbols.Mod))
        {
            return DeduceBinaryHelper(node, scope, context);
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
        else if (node.Calls(CodeSymbols.As))
        {
            if (TypenameTable.ContainsKey(node.Args[1].Name.Name))
            {
                var typName = LNode.Id(TypenameTable[node.Args[1].Name.Name]);
                return Deduce(typName, scope, context);
            }

            return Deduce(node.Args[1], scope, context);
        }
        else if (node.Calls(CodeSymbols.Dot))
        {
            var qualified = ConversionUtils.GetQualifiedName(node);
            var resolved = context.Binder.ResolveTypes(qualified).FirstOrDefault();

            if (resolved == null)
            {
                var left = Deduce(node.Args[0], scope, context); //Todo: implement deducing for members
            }

            return resolved;
        }
        else if (node.Calls(CodeSymbols.ColonColon))
        {
            var type = Deduce(node.Args[0], scope, context);
            var fnName = node.Args[1].Name;

            var methods = type.Methods.Where(_ => _.Name.ToString() == fnName.ToString());
            var deducedArgs = new List<IType>();

            foreach (var arg in node.Args[1].Args)
            {
                deducedArgs.Add(Deduce(arg, scope, context));
            }

            methods = methods.Where(_ => _.Parameters.Count == deducedArgs.Count);

            if (methods.Any())
            {
                foreach (var method in methods)
                {
                    var methodPs = string.Join(',',
                        method.Parameters.Select(_ => _.Type.FullName.ToString()));

                    var deducedPs = string.Join(',',
                        deducedArgs.Select(_ => _.FullName.ToString()));

                    if (methodPs == deducedPs)
                    {
                        return method.ReturnParameter.Type;
                    }
                }
            }
            else
            {
                context.AddError(node, $"Mismatching Parameter count: {type.FullName.ToString() + "::" + fnName}()");
            }
        }

        return null;
    }

    private static IType DeduceUnary(LNode node, Scope scope, CompilerContext context)
    {
        var left = Deduce(node.Args[0], scope, context);

        if (left.TryGetOperator(node.Name.Name, out var opMethod, left))
        {
            return opMethod.ReturnParameter.Type;
        }

        if (node.Calls(CodeSymbols._AddressOf))
        {
            return left.MakePointerType(PointerKind.Transient);
        }
        else if (node.Calls(CodeSymbols.Mul))
        {
            if (left is PointerType pt)
            {
                return pt.ElementType;
            }

            context.AddError(node, "Cannot dereference non pointer type");
        }
        else if (node.Calls(CodeSymbols.Not))
        {
            ExpectType(node.Args[0], scope, context, context.Environment.Boolean);

            return context.Environment.Boolean;
        }
        else if (node.Calls(CodeSymbols.NotBits))
        {
            return Deduce(node.Args[0], scope, context);
        }
        else if (node.Calls(CodeSymbols._Negate))
        {
            return NotExpectType(node.Args[0], scope, context, context.Environment.Boolean);
        }

        return null;
    }

    private static IType DeduceBinaryHelper(LNode node, Scope scope, CompilerContext context)
    {
        var left = Deduce(node.Args[0], scope, context);
        var right = Deduce(node.Args[1], scope, context);

        if (left.TryGetOperator(node.Name.Name, out var opMethod, left, right))
        {
            return opMethod.ReturnParameter.Type;
        }

        if (left != right)
        {
            if (left.IsPointerType())
            {
                ExpectType(node.Args[1], scope, context, context.Environment.Int32);

                return left;
            }
            else if (right.IsPointerType())
            {
                ExpectType(node.Args[0], scope, context, context.Environment.Int32);

                return right;
            }

            context.AddError(node, "Type mismatch");
            return null;
        }

        return left;
    }
}