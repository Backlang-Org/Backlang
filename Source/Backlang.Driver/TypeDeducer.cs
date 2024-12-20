﻿using Backlang.Codeanalysis.Core;
using Backlang.Contracts.TypeSystem;

namespace Backlang.Driver;

public static class TypeDeducer
{
    public static readonly ImmutableDictionary<string, Symbol> TypenameTable = new Dictionary<string, Symbol>
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
        ["string"] = CodeSymbols.String
    }.ToImmutableDictionary();

    //ToDo: check for implicit cast
    public static IType Deduce(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        if (ImplementationStage.LiteralTypeMap.ContainsKey(node.Name))
        {
            return ImplementationStage.GetLiteralType(node, context, scope, modulename);
        }

        if (TypenameTable.ContainsKey(node.Name.Name))
        {
            return Deduce(LNode.Id(TypenameTable[node.Name.Name]), scope, context, modulename);
        }

        if (node.Calls(CodeSymbols.Typeof))
        {
            return Utils.ResolveType(context.Binder, typeof(Type));
        }

        if (node.Calls(Symbols.Unit) && node is var (_, value, unit))
        {
            return DeduceUnitType(scope, context, modulename, value, unit);
        }

        if (node.Calls(CodeSymbols.As) && node is var (_, expr, castType))
        {
            return Deduce(castType, scope, context, modulename);
        }

        if (node.ArgCount == 1 && node.Calls(CodeSymbols.Default))
        {
            if (node is var (_, (_, (_, type))))
            {
                return Deduce(type, scope, context, modulename);
            }
        }
        else if (node.Calls(CodeSymbols.New))
        {
            if (node is var (_, call))
            {
                return Deduce(call.Target, scope, context, modulename);
            }
        }
        else if (node.Calls(CodeSymbols.Tuple))
        {
            return DeduceTuple(node, scope, context, modulename);
        }
        else if (node.Calls(CodeSymbols.Array))
        {
            return DeduceArray(node, scope, context, modulename);
        }
        else if (node.ArgCount == 1 && node.Name.Name.StartsWith("'"))
        {
            return DeduceUnary(node, scope, context, modulename);
        }
        else if (node.ArgCount == 2 && node.Name.Name.StartsWith("'"))
        {
            return DeduceBinary(node, scope, context, modulename);
        }
        else if (node.IsId || node.IsCall)
        {
            if (scope.TryGet<ScopeItem>(node.Name.Name, out var item))
            {
                return item?.Type;
            }

            var type = TypeInheritanceStage.ResolveTypeWithModule(node, context, modulename);

            if (type != null)
            {
                return type;
            }

            var suggestion = LevensteinDistance.Suggest(node.Name.Name, scope.GetAllScopeNames());

            context.AddError(node, $"{node.Name} cannot be resolved. Did you mean '{suggestion}'?");
        }

        return null;
    }

    public static IType DeduceFunctionReturnType(LNode funcDefinition, CompilerContext context, Scope scope,
        QualifiedName modulename)
    {
        var returnNodes = funcDefinition.Descendants().Where(_ => _.Calls(CodeSymbols.Return)).ToArray();

        if (!returnNodes.Any())
        {
            return Utils.ResolveType(context.Binder, typeof(void));
        }

        if (returnNodes.Length == 1 && returnNodes[0].ArgCount == 1)
        {
            return Deduce(returnNodes[0][0], scope, context, modulename);
        }

        var types = returnNodes.Where(_ => _.ArgCount > 0).Select(_ => Deduce(_[0], scope, context, modulename));

        if (!types.Any())
        {
            return null;
        }

        var aggregatedCommonType = types.Aggregate(FindCommonType);

        if (aggregatedCommonType != null)
        {
            return aggregatedCommonType;
        }

        context.AddError(funcDefinition, ErrorID.DeducingTypeNotPossible);
        return context.Environment.Void;
    }

    private static IType FindCommonType(IType first, IType second)
    {
        if (first == second)
        {
            return first;
        }

        if (first.IsAssignableTo(second))
        {
            return first;
        }

        if (second.IsAssignableTo(first))
        {
            return second;
        }

        if (second.BaseTypes.Count > 0 && second.BaseTypes[0] == first)
        {
            return first;
        }

        if (first.BaseTypes.Count > 0 && first.BaseTypes[0] == second)
        {
            return second;
        }

        if (first.BaseTypes.Count > 0 && second.BaseTypes.Count > 0)
        {
            if (first.BaseTypes[0].FullName.ToString() == "System.ValueType" ||
                second.BaseTypes[0].FullName.ToString() == "System.ValueType")
            {
                return null;
            }

            if (first.BaseTypes[0].FullName.ToString() == "System.Object" ||
                second.BaseTypes[0].FullName.ToString() == "System.Object")
            {
                return null;
            }

            if (first.BaseTypes[0] == second.BaseTypes[0])
            {
                return first.BaseTypes[0];
            }

            return FindCommonType(first.BaseTypes[0], second.BaseTypes[0]);
        }

        return null;
    }

    public static void ExpectType(LNode node, Scope scope, CompilerContext context, QualifiedName modulename,
        IType expectedType)
    {
        var deducedType = Deduce(node, scope, context, modulename);

        if (deducedType != expectedType)
        {
            context.AddError(node, $"Type Mismatch. Expected {expectedType}, got {deducedType}");
        }
    }

    public static IType NotExpectType(LNode node, Scope scope, CompilerContext context, QualifiedName modulename,
        IType expectedType)
    {
        var deducedType = Deduce(node, scope, context, modulename);

        if (deducedType == expectedType)
        {
            context.AddError(node, $"{expectedType} is not allowed here");
        }

        return deducedType;
    }

    private static IType DeduceUnitType(Scope scope, CompilerContext context, QualifiedName modulename, LNode value,
        LNode unit)
    {
        var resolvedUnit = TypeInheritanceStage.ResolveTypeWithModule(unit, context, modulename);

        if (!Utils.IsUnitType(context, resolvedUnit))
        {
            context.AddError(unit, $"{resolvedUnit} is not a unit type");
        }

        return new UnitType(Deduce(value, scope, context, modulename), resolvedUnit);
    }

    private static IType DeduceArray(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        //ToDo: Make deducing array type better
        var rank = GetArrayRank(node);

        while (node.ArgCount > 0 && node[0].Calls(CodeSymbols.Array))
        {
            node = node[0];
        }

        return context.Environment.MakeArrayType(Deduce(node.Args[0], scope, context, modulename), rank);
    }

    private static int GetArrayRank(LNode node)
    {
        var rank = 0;

        while (node.Calls(CodeSymbols.Array))
        {
            rank++;

            if (node.ArgCount > 0)
            {
                node = node[0];
            }
        }

        return rank;
    }

    private static IType DeduceTuple(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        var tupleType = context.Binder.ResolveTypes(new SimpleName("Tuple`" + node.ArgCount).Qualify("System"))
            .FirstOrDefault();

        if (tupleType == null)
        {
            context.AddError(node, $"A tuple cannot have {node.ArgCount} arguments");
        }

        var generics = new List<IType>();

        foreach (var arg in node.Args)
        {
            generics.Add(Deduce(arg, scope, context, modulename));
        }

        return tupleType.MakeGenericType(generics);
    }

    private static IType DeduceBinary(LNode node, Scope scope, CompilerContext context, QualifiedName moduleName)
    {
        if (node.Calls(CodeSymbols.Add) || node.Calls(CodeSymbols.Mul)
                                        || node.Calls(CodeSymbols.Div) || node.Calls(CodeSymbols.Sub) ||
                                        node.Calls(CodeSymbols.AndBits)
                                        || node.Calls(CodeSymbols.OrBits) || node.Calls((Symbol)"'^") ||
                                        node.Calls(CodeSymbols.Mod))
        {
            return DeduceBinaryHelper(node, scope, context, moduleName);
        }

        if (node.Calls(CodeSymbols.LT)
            || node.Calls(CodeSymbols.GT) || node.Calls(CodeSymbols.LE)
            || node.Calls(CodeSymbols.GE))
        {
            NotExpectType(node[0], scope, context, moduleName, context.Environment.Boolean);
            NotExpectType(node[1], scope, context, moduleName, context.Environment.Boolean);

            return context.Environment.Boolean;
        }

        if (node.Calls(CodeSymbols.Eq) || node.Calls(CodeSymbols.NotEq))
        {
            return context.Environment.Boolean;
        }

        if (node.Calls(CodeSymbols.As))
        {
            return DeduceExplicitCast(node, scope, context, moduleName);
        }

        if (node.Calls(CodeSymbols.Dot))
        {
            return DeduceMember(node, scope, context, moduleName);
        }

        if (node.Calls(CodeSymbols.ColonColon))
        {
            return DeduceStaticMethod(node, scope, context, moduleName);
        }

        return null;
    }

    private static IType DeduceStaticMethod(LNode node, Scope scope, CompilerContext context, QualifiedName moduleName)
    {
        var type = Deduce(node.Args[0], scope, context, moduleName);
        var fnName = node.Args[1].Name;

        var methods = type.Methods.Where(_ => _.Name.ToString() == fnName.ToString());
        var deducedArgs = new List<IType>();

        foreach (var arg in node.Args[1].Args)
        {
            deducedArgs.Add(Deduce(arg, scope, context, moduleName));
        }

        methods = methods.Where(_ => _.Parameters.Count == deducedArgs.Count);

        if (methods.Any())
        {
            foreach (var method in methods)
            {
                if (!ImplementationStage.MatchesParameters(method, deducedArgs))
                {
                    continue;
                }

                return method.ReturnParameter.Type;
            }
        }
        else
        {
            context.AddError(node, $"Mismatching Parameter count: {type.FullName + "::" + fnName}()");
        }

        return null;
    }

    private static IType DeduceMember(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        var qualified = ConversionUtils.GetQualifiedName(node);
        var resolved = context.Binder.ResolveTypes(qualified).FirstOrDefault();

        if (resolved == null)
        {
            var left = Deduce(node.Args[0], scope, context, modulename);
            var field = left.Fields.FirstOrDefault(_ => _.Name.ToString() == qualified.Name.ToString());

            if (field != null)
            {
                return field.FieldType;
            }

            var funcArgs = node[1].Args.Select(_ => Deduce(_, scope, context, modulename));
            var func = context.Binder.FindFunction(
                left + "::" + qualified.Name + "(" + string.Join(',', funcArgs) + ")");

            return func.ReturnParameter.Type;
        }

        return resolved;
    }

    private static IType DeduceExplicitCast(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        if (TypenameTable.ContainsKey(node.Args[1].Name.Name))
        {
            var typName = LNode.Id(TypenameTable[node.Args[1].Name.Name]);
            return Deduce(typName, scope, context, modulename);
        }

        return Deduce(node.Args[1], scope, context, modulename);
    }

    private static IType DeduceUnary(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        var left = Deduce(node.Args[0], scope, context, modulename);

        if (left.TryGetOperator(node.Name.Name, out var opMethod, left))
        {
            return opMethod.ReturnParameter.Type;
        }

        if (node.Calls(CodeSymbols._AddressOf))
        {
            return left.MakePointerType(PointerKind.Transient);
        }

        if (node.Calls(CodeSymbols._Dereference))
        {
            if (left is PointerType pt)
            {
                return pt.ElementType;
            }

            context.AddError(node, "Cannot dereference non pointer type");
        }
        else if (node.Calls(CodeSymbols.Not))
        {
            ExpectType(node.Args[0], scope, context, modulename, context.Environment.Boolean);

            return context.Environment.Boolean;
        }
        else if (node.Calls(CodeSymbols.NotBits))
        {
            return Deduce(node.Args[0], scope, context, modulename);
        }
        else if (node.Calls(CodeSymbols._Negate))
        {
            return NotExpectType(node.Args[0], scope, context, modulename, context.Environment.Boolean);
        }

        return null;
    }

    private static IType DeduceBinaryHelper(LNode node, Scope scope, CompilerContext context, QualifiedName modulename)
    {
        var left = Deduce(node.Args[0], scope, context, modulename);
        var right = Deduce(node.Args[1], scope, context, modulename);

        if (left.TryGetOperator(node.Name.Name, out var opMethod, left, right))
        {
            return opMethod.ReturnParameter.Type;
        }

        if (left != right) //ToDo: Add implicit casting check
        {
            if (left is UnitType && right is not UnitType)
            {
                return left;
            }

            if (right is UnitType && left is not UnitType)
            {
                return right;
            }

            if (left.IsPointerType())
            {
                ExpectType(node.Args[1], scope, context, modulename, context.Environment.Int32);

                return left;
            }

            if (right.IsPointerType())
            {
                ExpectType(node.Args[0], scope, context, modulename, context.Environment.Int32);

                return right;
            }

            context.AddError(node, "Type mismatch");
            return null;
        }

        return left;
    }
}