using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Stages;

public sealed class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public static readonly Dictionary<string, Type> TypenameTable = new()
    {
        ["obj"] = typeof(object),

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
    };

    public static IType GetType(LNode type, CompilerContext context)
    {
        //function without return type set
        if (type == LNode.Missing || type.Args[0].Name.Name == "#") return ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void));

        if (type.Name == CodeSymbols.Fn)
        {
            string typename = string.Empty;
            typename = type.Args[0] == LNode.Missing ? "Action`" + (type.Args[2].ArgCount) : "Func`" + (type.Args[2].ArgCount + 1);

            var fnType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typename, "System");
            foreach (var garg in type.Args[2])
            {
                fnType.AddGenericParameter(new DescribedGenericParameter(fnType, garg.Name.Name.ToString())); //ToDo: replace primitive aliases with real .net typenames
            }

            return fnType;
        }

        var name = type.Args[0].Name.ToString().Replace("#", "");

        if (TypenameTable.ContainsKey(name))
        {
            return ClrTypeEnvironmentBuilder.ResolveType(context.Binder, TypenameTable[name]);
        }
        else
        {
            return ClrTypeEnvironmentBuilder.ResolveType(context.Binder, name, context.Assembly.Name.Qualify().FullName);
        }
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Assembly = new DescribedAssembly(new QualifiedName(context.OutputFilename));
        context.ExtensionsType = new DescribedType(new SimpleName(Names.Extensions).Qualify(context.Assembly.Name), context.Assembly)
        {
            IsStatic = true
        };

        foreach (var tree in context.Trees)
        {
            ConvertTypesOrInterfaces(context, tree);
            ConvertEnums(context, tree);
        }

        context.Assembly.AddType(context.ExtensionsType);
        context.Binder.AddAssembly(context.Assembly);

        return await next.Invoke(context);
    }

    private static void ConvertEnums(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
    {
        var enums = tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Enum);

        foreach (var enu in enums)
        {
            var name = enu.Args[0].Name;
            var members = enu.Args[2];

            var type = new DescribedType(new SimpleName(name.Name).Qualify(context.Assembly.Name), context.Assembly);
            type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("Enum").Qualify("System")).First());

            type.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            context.Assembly.AddType(type);
        }
    }

    private static void ConvertTypesOrInterfaces(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
    {
        var types = tree.Body.Where(_ => _.IsCall && (_.Name == CodeSymbols.Struct || _.Name == CodeSymbols.Class || _.Name == CodeSymbols.Interface));

        foreach (var st in types)
        {
            var name = st.Args[0].Name;
            var inheritances = st.Args[0].Args[1];
            var members = st.Args[0].Args[2];

            var type = new DescribedType(new SimpleName(name.Name).Qualify(context.Assembly.Name), context.Assembly);
            if (st.Name == CodeSymbols.Struct)
            {
                type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("ValueType").Qualify("System")).First()); // make it a struct
            }
            else if (st.Name == CodeSymbols.Interface)
            {
                type.AddAttribute(FlagAttribute.InterfaceType);
            }

            Utils.SetAccessModifier(st, type);
            SetOtherModifiers(st, type);

            context.Assembly.AddType(type);
        }
    }

    private static void SetOtherModifiers(LNode node, DescribedType type)
    {
        if (node.Attrs.Contains(LNode.Id(CodeSymbols.Static)))
        {
            type.IsStatic = true;
        }
        if (node.Attrs.Contains(LNode.Id(CodeSymbols.Abstract)))
        {
            type.IsAbstract = true;
        }
    }
}