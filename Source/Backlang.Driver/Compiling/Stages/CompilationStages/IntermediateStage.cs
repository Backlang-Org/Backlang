using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Backlang.Contracts.Scoping.Items;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public sealed class IntermediateStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Assembly = new DescribedAssembly(new QualifiedName(context.OutputFilename.Replace(".dll", "")));
        context.ExtensionsType = new DescribedType(new SimpleName(Names.Extensions).Qualify(string.Empty), context.Assembly)
        {
            IsStatic = true,
            IsPublic = true
        };

        foreach (var tree in context.Trees)
        {
            var modulename = ConversionUtils.GetModuleName(tree);

            foreach (var st in tree.Body)
            {
                if (st.Calls(CodeSymbols.Struct) || st.Calls(CodeSymbols.Class) || st.Calls(CodeSymbols.Interface))
                {
                    ConvertTypeOrInterface(context, st, modulename, context.GlobalScope);
                }
                else if (st.Calls(CodeSymbols.Enum))
                {
                    ConvertEnum(context, st, modulename);
                }
                else if (st.Calls(Symbols.DiscriminatedUnion))
                {
                    ConvertDiscriminatedUnion(context, st, modulename);
                }
            }
        }

        context.Assembly.AddType(context.ExtensionsType);
        context.Binder.AddAssembly(context.Assembly);

        return await next.Invoke(context);
    }

    private static void ConvertEnum(CompilerContext context, LNode @enum, QualifiedName modulename)
    {
        if (@enum is (_, (_, var nameNode, var typeNode, var membersNode)))
        {
            var name = nameNode.Name;

            var type = new DescribedType(new SimpleName(name.Name).Qualify(modulename), context.Assembly);
            type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("Enum").Qualify("System")).First());

            type.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            context.Assembly.AddType(type);
        }
    }

    private static void ConvertTypeOrInterface(CompilerContext context, LNode st, QualifiedName modulename, Scope scope)
    {
        var name = st.Args[0].Name;

        var type = new DescribedType(new SimpleName(name.Name).Qualify(modulename), context.Assembly);
        if (st.Name == CodeSymbols.Struct)
        {
            type.AddBaseType(context.Binder.ResolveTypes(new SimpleName("ValueType").Qualify("System")).First()); // make it a struct
        }
        else if (st.Name == CodeSymbols.Interface)
        {
            type.AddAttribute(FlagAttribute.InterfaceType);
        }

        ConversionUtils.SetAccessModifier(st, type);
        SetOtherModifiers(st, type);

        if (scope.Add(new TypeScopeItem { Name = name.Name, Type = type, SubScope = scope.CreateChildScope() }))
        {
            context.Assembly.AddType(type);
        }
        else
        {
            context.AddError(st, $"Type {name.Name} is already defined.");
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

    private void ConvertDiscriminatedUnion(CompilerContext context, LNode discrim, QualifiedName modulename)
    {
        var name = discrim.Args[0].Name;

        var baseType = new DescribedType(new SimpleName(name.Name).Qualify(modulename), context.Assembly);
        ConversionUtils.SetAccessModifier(discrim, baseType);
        baseType.IsAbstract = true;

        context.Assembly.AddType(baseType);

        foreach (var type in discrim.Args[1].Args)
        {
            var discName = type.Args[0].Name;
            var discType = new DescribedType(new SimpleName(discName.Name).Qualify(modulename), context.Assembly);
            ConversionUtils.SetAccessModifier(discrim, discType);
            discType.AddBaseType(baseType);
            context.Assembly.AddType(discType);

            foreach (var field in type.Args[1].Args)
            {
                var fieldName = field.Args[1].Args[0].Name;
                var fieldType = new DescribedField(discType, new SimpleName(fieldName.Name),
                    false,
                    TypeInheritanceStage.ResolveTypeWithModule(
                        field.Args[0].Args[0].Args[0], context,
                        modulename, ConversionUtils.GetQualifiedName(field.Args[0].Args[0].Args[0])
                        )
                    );

                if (field.Attrs.Any(_ => _.Name == Symbols.Mutable))
                {
                    fieldType.AddAttribute(Attributes.Mutable);
                }
                fieldType.IsPublic = true;
                discType.AddField(fieldType);
            }

            IRGenerator.GenerateDefaultCtor(context, discType);
            IRGenerator.GenerateToString(context, discType);
        }
    }
}