using Backlang.Contracts.TypeSystem;
using Flo;
using Loyc.Geometry;

namespace Backlang.Driver.Compiling.Stages;

public sealed partial class TypeInheritanceStage : IHandler<CompilerContext, CompilerContext>
{
    public static readonly ImmutableDictionary<string, Type> TypenameTable = new Dictionary<string, Type>()
    {
        ["obj"] = typeof(object),
        ["none"] = typeof(void),

        ["bool"] = typeof(bool),

        ["u8"] = typeof(byte),
        ["u16"] = typeof(ushort),
        ["u32"] = typeof(uint),
        ["u64"] = typeof(ulong),

        ["i8"] = typeof(sbyte),
        ["i16"] = typeof(short),
        ["i32"] = typeof(int),
        ["i64"] = typeof(long),

        ["f16"] = typeof(Half),
        ["f32"] = typeof(float),
        ["f64"] = typeof(double),

        ["char"] = typeof(char),
        ["string"] = typeof(string),
    }.ToImmutableDictionary();

    public static IType ResolveTypeWithModule(LNode typeNode, CompilerContext context, QualifiedName modulename)
        => ResolveTypeWithModule(typeNode, context, modulename, ConversionUtils.GetQualifiedName(typeNode));

    public static IType ResolveTypeWithModule(LNode typeNode, CompilerContext context, QualifiedName modulename, QualifiedName fullName)
    {
        bool isPointer;
        PointerKind pointerKind = PointerKind.Transient;

        if (fullName.FullyUnqualifiedName is PointerName pName)
        {
            isPointer = true;
            pointerKind = pName.Kind;
            fullName = pName.ElementName;
        }
        else
        {
            isPointer = false;
        }

        IType resolvedType;

        if (context.GlobalScope.TypeAliases.ContainsKey(fullName.ToString()))
        {
            resolvedType = context.GlobalScope.TypeAliases[fullName.ToString()];
        }
        else if (TypenameTable.ContainsKey(fullName.ToString()))
        {
            resolvedType = Utils.ResolveType(context.Binder, TypenameTable[fullName.FullName]);

            if (typeNode is (_, (_, _, (_, var unit))) && unit != LNode.Missing)
            {
                ResolveUnitType(context, modulename, ref resolvedType, unit);
            }
        }
        else if (typeNode is ("#type?", var nullableArg))
        {
            resolvedType = ResolveNullableType(nullableArg, context, modulename);
        }
        else if (fullName is ("System", var func) && (func.StartsWith("Action") || func.StartsWith("Func")))
        {
            resolvedType = ResolveFunctionType(typeNode, context, modulename, func);
        }
        else if (typeNode.Calls(CodeSymbols.Tuple))
        {
            resolvedType = ResolveTupleType(typeNode, context, modulename);
        }
        else if (typeNode.Calls(CodeSymbols.Array))
        {
            resolvedType = ResolveArrayType(typeNode, context, modulename);
        }
        else
        {
            resolvedType = context.Binder.ResolveTypes(fullName).FirstOrDefault();

            if (resolvedType == null)
            {
                resolvedType = context.Binder.ResolveTypes(fullName.Qualify(modulename)).FirstOrDefault();
            }

            if (resolvedType == null)
            {
                if (context.FileScope.ImportetNamespaces.TryGetValue(typeNode.Range.Source.FileName, out var importedNamespaces))
                {
                    ResolveImportedType(typeNode, context, ref fullName, ref resolvedType);
                }

                if (resolvedType == null && !string.IsNullOrEmpty(fullName.ToString()))
                {
                    context.AddError(typeNode, $"Type {fullName} cannot be found");
                    return null;
                }
            }
        }

        if (isPointer)
        {
            resolvedType = resolvedType.MakePointerType(pointerKind);
        }

        return resolvedType;
    }

    private static IType ResolveNullableType(LNode nullableArg, CompilerContext context, QualifiedName modulename)
    {
        var tupleType = Utils.ResolveType(context.Binder, $"Nullable`1", "System");

        var innerType = ResolveTypeWithModule(nullableArg, context, modulename);
        
        return tupleType.MakeGenericType(new List<IType>() { innerType });
    }

    private static void ResolveImportedType(LNode typeNode, CompilerContext context, ref QualifiedName fullName, ref IType resolvedType)
    {
        var namespaceImport = context.FileScope.ImportetNamespaces[typeNode.Range.Source.FileName];

        foreach (var importedNs in namespaceImport.ImportedNamespaces)
        {
            var tmpName = fullName.Qualify(importedNs);

            resolvedType = context.Binder.ResolveTypes(tmpName).FirstOrDefault();

            if (resolvedType != null) break;
        }
    }

    private static IType ResolveArrayType(LNode typeNode, CompilerContext context, QualifiedName modulename)
    {
        IType resolvedType;
        var arrType = ResolveTypeWithModule(typeNode[0], context, modulename);
        resolvedType = context.Environment.MakeArrayType(arrType, (int)typeNode[1].Value);
        return resolvedType;
    }

    private static IType ResolveTupleType(LNode typeNode, CompilerContext context, QualifiedName modulename)
    {
        IType resolvedType;
        var tupleType = Utils.ResolveType(context.Binder, $"Tuple`{typeNode.ArgCount}", "System");

        var tupleArgs = new List<IType>();
        foreach (var garg in typeNode.Args)
        {
            tupleArgs.Add(ResolveTypeWithModule(garg, context, modulename));
        }

        resolvedType = tupleType.MakeGenericType(tupleArgs);
        return resolvedType;
    }

    private static IType ResolveFunctionType(LNode typeNode, CompilerContext context, QualifiedName modulename, string func)
    {
        IType resolvedType;
        var fnType = Utils.ResolveType(context.Binder, func, "System");

        var funcArgs = new List<IType>();
        foreach (var garg in typeNode.Args[2].Args)
        {
            funcArgs.Add(ResolveTypeWithModule(garg, context, modulename));
        }

        if (func.StartsWith("Func"))
        {
            funcArgs.Add(ResolveTypeWithModule(typeNode.Args[0], context, modulename));
        }

        resolvedType = fnType.MakeGenericType(funcArgs);
        return resolvedType;
    }

    private static void ResolveUnitType(CompilerContext context, QualifiedName modulename, ref IType resolvedType, LNode unit)
    {
        if (unit is (_, (_, var u)))
        {
            var resolvedUnit = ResolveTypeWithModule(u, context, modulename);

            if (!Utils.IsUnitType(context, resolvedUnit))
            {
                context.AddError(u, $"{resolvedUnit} is not a unit type");
            }

            resolvedType = new UnitType(resolvedType, resolvedUnit);
        }
    }
}