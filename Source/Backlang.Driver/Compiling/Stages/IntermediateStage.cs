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
        if (type == LNode.Missing) return ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void));

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
        context.Assembly = new DescribedAssembly(new QualifiedName("Compilation"));
        context.ExtensionsType = new DescribedType(new SimpleName("__Extensions").Qualify(context.Assembly.Name), context.Assembly);

        foreach (var tree in context.Trees)
        {
            ConvertStructs(context, tree);
        }

        context.Assembly.AddType(context.ExtensionsType);
        context.Binder.AddAssembly(context.Assembly);

        return await next.Invoke(context);
    }

    private static void ConvertStructMembers(LNode members, DescribedType type, CompilerContext context)
    {
        foreach (var member in members.Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                var mtype = GetType(member.Args[0], context);

                var mvar = member.Args[1];
                var mname = mvar.Args[0].Name;

                var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

                type.AddField(field);
            }
        }
    }

    private static void ConvertStructs(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
    {
        var structs = tree.Body.Where(_ => _.IsCall && (_.Name == CodeSymbols.Struct || _.Name == CodeSymbols.Class));

        foreach (var st in structs)
        {
            var name = st.Args[0].Name;
            var inheritances = st.Args[1];
            var members = st.Args[2];

            var type = new DescribedType(new SimpleName(name.Name).Qualify(context.Assembly.Name), context.Assembly);
            if(st.Name == CodeSymbols.Struct)
            {
                type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("ValueType").Qualify("System")).First()); // make it a struct
            }

            type.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            context.Assembly.AddType(type);
        }
    }
}