using Backlang.Contracts.TypeSystem;
using Flo;

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
        if (TypenameTable.ContainsKey(fullName.ToString()))
        {
            resolvedType = Utils.ResolveType(context.Binder, TypenameTable[fullName.FullName]);

            if (typeNode is (_, (_, _, (_, var unit))) && unit != LNode.Missing)
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
        else if (fullName is ("System", var func) && (func.StartsWith("Action") || func.StartsWith("Func")))
        {
            var fnType = Utils.ResolveType(context.Binder, func, "System");
            foreach (var garg in typeNode.Args[2])
            {
                fnType.AddGenericParameter(new DescribedGenericParameter(fnType, garg.Name.Name.ToString())); //ToDo: replace primitive aliases with real .net typenames
            }
            resolvedType = fnType;
        }
        else if (typeNode.Calls(CodeSymbols.Tuple))
        {
            var tupleType = context.Binder.ResolveTypes(new SimpleName("Tuple`" + typeNode.ArgCount).Qualify("System")).FirstOrDefault();

            var tupleArgs = new List<IType>();
            foreach (var arg in typeNode.Args)
            {
                tupleArgs.Add(ResolveTypeWithModule(arg, context, modulename));
            }

            return tupleType.MakeGenericType(tupleArgs);
        }
        else if (typeNode.Calls(CodeSymbols.Array))
        {
            resolvedType = ResolveTypeWithModule(typeNode[0], context, modulename);
            resolvedType = context.Environment.MakeArrayType(resolvedType, (int)typeNode[1].Value);
        }
        else
        {
            resolvedType = context.Binder.ResolveTypes(fullName).FirstOrDefault();

            if (resolvedType == null)
            {
                resolvedType = context.Binder.ResolveTypes(fullName.Qualify(modulename)).FirstOrDefault();
            }
        }

        if (isPointer)
        {
            resolvedType = resolvedType.MakePointerType(pointerKind);
        }

        return resolvedType;
    }
}