using Backlang.Contracts;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;
using System.Collections.Immutable;

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
        => ResolveTypeWithModule(typeNode, context, modulename, Utils.GetQualifiedName(typeNode));

    public static IType ResolveTypeWithModule(LNode typeNode, CompilerContext context, QualifiedName modulename, QualifiedName fullName)
    {
        bool isPointer;
        if (fullName.FullyUnqualifiedName is PointerName pName)
        {
            isPointer = true;
            fullName = pName.ElementName;
        }
        else
        {
            isPointer = false;
        }

        IType resolvedType;
        if (TypenameTable.ContainsKey(fullName.ToString()))
        {
            resolvedType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, TypenameTable[fullName.FullName]);
        }
        else if (fullName is ("System", var func) && (func.StartsWith("Action") || func.StartsWith("Func")))
        {
            var fnType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, func, "System");
            foreach (var garg in typeNode.Args[2])
            {
                fnType.AddGenericParameter(new DescribedGenericParameter(fnType, garg.Name.Name.ToString())); //ToDo: replace primitive aliases with real .net typenames
            }
            resolvedType = fnType;
        }
        else
        {
            resolvedType = context.Binder.ResolveTypes(fullName).FirstOrDefault();

            if (resolvedType == null)
            {
                resolvedType = context.Binder.ResolveTypes(fullName.Qualify(modulename)).FirstOrDefault();

                if (resolvedType == null)
                {
                    context.AddError(typeNode, $"Type {fullName} cannot be found");
                }
            }
        }

        if (isPointer)
        {
            resolvedType = resolvedType.MakePointerType(PointerKind.Transient);
        }

        return resolvedType;
    }
}